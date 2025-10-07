using AutoSys.Data;
using AutoSys.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization; // AÑADIR ESTE USING
using System;

namespace AutoSys.Controllers
{
    [Authorize(Roles = "Administrador,Recepcionista,Mecanico")] // <-- AÑADIR ESTA LÍNEA
    public class IngresoController : Controller
    {
        private readonly AutoSysDbContext _context;
        private readonly IWebHostEnvironment _env;

        public IngresoController(AutoSysDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // Muestra el formulario inicial para crear un ingreso
        [Authorize(Roles = "Administrador,Recepcionista")]
        public IActionResult Create()
        {
            ViewBag.Vehiculos = _context.Vehiculos
                .Include(v => v.Cliente)
                .Select(v => new
                {
                    v.Id,
                    Descripcion = $"{v.Patente} - {v.Marca} {v.Modelo} ({v.Cliente.Nombre} {v.Cliente.Apellido})"
                })
                .ToList();

            return View();
        }

        // Procesa los datos ingresados en Create y muestra la vista de confirmación
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Ingreso ingreso, IFormFile? Foto, string? FotoTempPath)
        {
            // Validación de existencia de ingreso activo para el vehículo
            bool ingresoActivo = await _context.Ingresos
                .AnyAsync(i => i.VehiculoId == ingreso.VehiculoId && i.FechaEgreso == null);

            if (ingresoActivo)
                ModelState.AddModelError("VehiculoId", "Este vehículo ya tiene una orden de ingreso activa.");

            var vehiculo = await _context.Vehiculos
                .Include(v => v.Cliente)
                .FirstOrDefaultAsync(v => v.Id == ingreso.VehiculoId);

            if (vehiculo == null)
                ModelState.AddModelError("VehiculoId", "El vehículo seleccionado no es válido.");

            if (!ModelState.IsValid)
            {
                ViewBag.Vehiculos = _context.Vehiculos
                    .Include(v => v.Cliente)
                    .Select(v => new
                    {
                        v.Id,
                        Descripcion = $"{v.Patente} - {v.Marca} {v.Modelo} ({v.Cliente.Nombre} {v.Cliente.Apellido})"
                    })
                    .ToList();

                ViewBag.FotoTempPath = FotoTempPath; // Por si vuelve con errores y ya tenía una imagen temporal
                return View(ingreso);
            }

            ingreso.Vehiculo = vehiculo;

            string? rutaTemp = FotoTempPath;

            // Procesamiento de nueva imagen (si fue cargada)
            if (Foto != null && Foto.Length > 0)
            {
                string tempFolder = Path.Combine(_env.WebRootPath, "temp");
                Directory.CreateDirectory(tempFolder);

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(Foto.FileName);
                string filePath = Path.Combine(tempFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await Foto.CopyToAsync(stream);
                }

                rutaTemp = "/temp/" + fileName;
            }

            ViewBag.FotoTempPath = rutaTemp;
            return View("Confirmar", ingreso);
        }

        // Confirmación final del ingreso: guarda en base de datos
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirmado(Ingreso ingreso, string? FotoTempPath)
        {
            ingreso.FechaIngreso = DateTime.Now;
            ingreso.FechaEgreso = null;
            // Estado por defecto si no vino seteado (backward compatible)
            if (string.IsNullOrWhiteSpace(ingreso.Estado))
            {
                ingreso.Estado = "En revisión";
            }

            if (!string.IsNullOrEmpty(FotoTempPath))
            {
                string nombreArchivo = Path.GetFileName(FotoTempPath);
                string rutaTempCompleta = Path.Combine(_env.WebRootPath, "temp", nombreArchivo);
                string carpetaUploads = Path.Combine(_env.WebRootPath, "uploads");
                Directory.CreateDirectory(carpetaUploads);

                string rutaFinal = Path.Combine(carpetaUploads, nombreArchivo);
                if (System.IO.File.Exists(rutaTempCompleta))
                {
                    System.IO.File.Move(rutaTempCompleta, rutaFinal);
                    ingreso.FotoPath = "/uploads/" + nombreArchivo;
                }
            }

            _context.Ingresos.Add(ingreso);
            await _context.SaveChangesAsync();

            // Nuevo: registrar también la foto en la tabla FotosVehiculo si existe
            if (!string.IsNullOrEmpty(ingreso.FotoPath))
            {
                var foto = new FotoVehiculo
                {
                    IngresoId = ingreso.Id,
                    Tipo = "Ingreso",
                    RutaArchivo = ingreso.FotoPath,
                    FechaCarga = DateTime.Now
                };
                _context.FotosVehiculo.Add(foto);
                await _context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = "¡El ingreso del vehículo se ha registrado correctamente!";
            return RedirectToAction("Detalle", new { id = ingreso.Id });
        }

        // Muestra el detalle de un ingreso específico
        public async Task<IActionResult> Detalle(int id)
        {
            var ingreso = await _context.Ingresos
                .Include(i => i.Vehiculo!)
                .ThenInclude(v => v.Cliente)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (ingreso == null)
                return NotFound();

            return View(ingreso);
        }

        // Actualiza el estado del ingreso
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActualizarEstado(int id, string estado)
        {
            if (string.IsNullOrWhiteSpace(estado))
            {
                TempData["ErrorMessage"] = "Debe seleccionar un estado válido.";
                return RedirectToAction("Detalle", new { id });
            }

            var ingreso = await _context.Ingresos.FirstOrDefaultAsync(i => i.Id == id);
            if (ingreso == null)
            {
                return NotFound();
            }

            ingreso.Estado = estado;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Estado actualizado correctamente.";
            return RedirectToAction("Detalle", new { id });
        }

        // Muestra todos los ingresos ordenados por fecha
        public async Task<IActionResult> Index()
        {
            var ingresos = await _context.Ingresos
                .Include(i => i.Vehiculo!)
                .ThenInclude(v => v.Cliente)
                .OrderByDescending(i => i.FechaIngreso)
                .ToListAsync();

            return View(ingresos);
        }

        // Acción para editar desde la vista de confirmación (regresa a Create con los datos precargados)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditarDesdeConfirmacion(int VehiculoId, string Diagnostico, string? FotoTempPath)
        {
            var ingreso = new Ingreso
            {
                VehiculoId = VehiculoId,
                Diagnostico = Diagnostico
            };
            ViewBag.Vehiculos = _context.Vehiculos
                .Include(v => v.Cliente)
                .Select(v => new
                {
                    v.Id,
                    Descripcion = $"{v.Patente} - {v.Marca} {v.Modelo} ({v.Cliente.Nombre} {v.Cliente.Apellido})"
                })
                .ToList();

            ViewBag.FotoTempPath = FotoTempPath;

            return View("Create", ingreso);
        }
    }
}