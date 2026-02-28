using AutoSys.Data;
using AutoSys.Models;
using AutoSys.Patterns.Observer;
using AutoSys.Services;
using AutoSys.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Security.Claims;

namespace AutoSys.Controllers
{
    [Authorize(Roles = "Administrador,Recepcionista,Mecanico")]
    public class IngresoController : Controller
    {
        private readonly AutoSysDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly EventSubject _eventSubject;
        private readonly INotificationService _notificationService;
        private readonly ILogger<IngresoController> _logger;
        private readonly IPermissionService _permissionService;
        private readonly IAuditService _auditService;

        public IngresoController(AutoSysDbContext context,
                                 IWebHostEnvironment env,
                                 EventSubject eventSubject,
                                 INotificationService notificationService,
                                 ILogger<IngresoController> logger,
                                 IPermissionService permissionService,
                                 IAuditService auditService)
        {
            _context = context;
            _env = env;
            _eventSubject = eventSubject;
            _notificationService = notificationService;
            _logger = logger;
            _permissionService = permissionService;
            _auditService = auditService;

            // PATRÓN OBSERVER: Adjuntar observadores al sujeto
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            _eventSubject.Attach(new EmailNotificationObserver(_notificationService,
                loggerFactory.CreateLogger<EmailNotificationObserver>()));
            _eventSubject.Attach(new LoggerObserver(loggerFactory.CreateLogger<LoggerObserver>()));
        }

        // ── ENDPOINT API: Búsqueda de vehículos (autocomplete estilo Google) ──
        [HttpGet]
        public IActionResult BuscarVehiculos(string? termino)
        {
            var query = _context.Vehiculos
                .Include(v => v.Cliente)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(termino))
            {
                var term = termino.Trim().ToLower();
                query = query.Where(v =>
                    v.Patente.ToLower().Contains(term) ||
                    v.Marca.ToLower().Contains(term) ||
                    v.Modelo.ToLower().Contains(term) ||
                    (v.Cliente != null && v.Cliente.Nombre.ToLower().Contains(term)) ||
                    (v.Cliente != null && v.Cliente.Apellido.ToLower().Contains(term))
                );
            }

            var resultados = query
                .OrderBy(v => v.Patente)
                .Select(v => new
                {
                    id = v.Id,
                    patente = v.Patente,
                    marca = v.Marca,
                    modelo = v.Modelo,
                    clienteNombre = v.Cliente != null
                        ? v.Cliente.Nombre + " " + v.Cliente.Apellido
                        : "Sin cliente",
                    descripcion = v.Patente + " - " + v.Marca + " " + v.Modelo +
                        " (" + (v.Cliente != null ? v.Cliente.Nombre + " " + v.Cliente.Apellido : "Sin cliente") + ")"
                })
                .ToList();

            return Json(resultados);
        }

        [Authorize(Roles = "Administrador,Recepcionista,Mecanico")]
        [RequirePermiso("CrearIngresos")]
        public async Task<IActionResult> Create()
        {
            ViewBag.ServiciosFijos = await _context.ServiciosFijos.ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermiso("CrearIngresos")]
        public async Task<IActionResult> Create(Ingreso ingreso, IFormFile? Foto, string? FotoTempPath)
        {
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
                ViewBag.ServiciosFijos = await _context.ServiciosFijos.ToListAsync();
                ViewBag.FotoTempPath = FotoTempPath; // Por si vuelve con errores y ya tenía una imagen temporal
                return View(ingreso);
            }

            ingreso.Vehiculo = vehiculo;

            string? rutaTemp = FotoTempPath;

            if (Foto != null && Foto.Length > 0)
            {
                // Validar extensión
                var ext = Path.GetExtension(Foto.FileName).ToLowerInvariant();
                var allowed = new HashSet<string> { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                if (!allowed.Contains(ext))
                {
                    ModelState.AddModelError("Foto", "Solo se permiten imágenes (.jpg, .jpeg, .png, .gif, .webp).");
                    return View(ingreso);
                }
                // Validar tamaño (5 MB)
                if (Foto.Length > 5_242_880)
                {
                    ModelState.AddModelError("Foto", "La imagen no puede superar 5 MB.");
                    return View(ingreso);
                }

                string tempFolder = Path.Combine(_env.WebRootPath, "temp");
                Directory.CreateDirectory(tempFolder);

                string fileName = Guid.NewGuid().ToString() + ext;
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirmado(Ingreso ingreso, string? FotoTempPath)
        {
            ingreso.FechaIngreso = DateTime.Now;
            ingreso.FechaEgreso = null;

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

            // AUDITORÍA
            var rolActual = User.IsInRole("Administrador") ? "Administrador" : User.IsInRole("Recepcionista") ? "Recepcionista" : "Mecanico";
            var vehiculoInfo = await _context.Vehiculos.Include(v => v.Cliente).FirstOrDefaultAsync(v => v.Id == ingreso.VehiculoId);
            await _auditService.RegistrarAsync(User.Identity?.Name ?? "desconocido", rolActual, "Ingreso", "Crear",
                $"Ingreso creado para vehículo {vehiculoInfo?.Patente ?? "N/A"} - {vehiculoInfo?.Cliente?.Nombre} {vehiculoInfo?.Cliente?.Apellido}",
                ingreso.Id, vehiculoInfo?.Patente, HttpContext.Connection.RemoteIpAddress?.ToString());

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
            return RedirectToAction("Detalle", "Ingreso", new { id = ingreso.Id });
        }

        public async Task<IActionResult> Detalle(int id)
        {
            var ingreso = await _context.Ingresos
                .Include(i => i.Vehiculo!)
                .ThenInclude(v => v.Cliente)
                .Include(i => i.ServicioFijo)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (ingreso == null)
                return NotFound();

            // ── Calcular permisos efectivos para la vista ──
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var rol = User.IsInRole("Administrador") ? "Administrador"
                    : User.IsInRole("Recepcionista") ? "Recepcionista"
                    : "Mecanico";

            // Admin y Recepcionista siempre pueden; Mecánico solo si tiene el permiso delegado
            bool puedeActualizarEstado = User.IsInRole("Administrador") || User.IsInRole("Recepcionista");
            bool puedeCrearIngreso     = User.IsInRole("Administrador") || User.IsInRole("Recepcionista");

            if (!puedeActualizarEstado && !string.IsNullOrEmpty(userId))
                puedeActualizarEstado = await _permissionService.TienePermisoEfectivoAsync(userId, rol, nameof(AutoSys.Models.UserPermission.ActualizarEstadoIngresos));

            if (!puedeCrearIngreso && !string.IsNullOrEmpty(userId))
                puedeCrearIngreso = await _permissionService.TienePermisoEfectivoAsync(userId, rol, nameof(AutoSys.Models.UserPermission.CrearIngresos));

            ViewBag.PuedeActualizarEstado = puedeActualizarEstado;
            ViewBag.PuedeCrearIngreso     = puedeCrearIngreso;

            return View(ingreso);
        }

        // Actualiza el estado del ingreso
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActualizarEstado(int id, string estado)
        {
            // Verificar permiso efectivo (Admin y Receptionist siempre OK; Mecánico solo si tiene permiso delegado)
            bool tienePermiso = User.IsInRole("Administrador") || User.IsInRole("Recepcionista");
            if (!tienePermiso)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
                tienePermiso = await _permissionService.TienePermisoEfectivoAsync(userId, "Mecanico",
                    nameof(AutoSys.Models.UserPermission.ActualizarEstadoIngresos));
            }

            if (!tienePermiso)
            {
                TempData["ErrorMessage"] = "No tiene permiso para actualizar el estado de ingresos.";
                return RedirectToAction("Detalle", new { id });
            }
            if (string.IsNullOrWhiteSpace(estado))
            {
                TempData["ErrorMessage"] = "Debe seleccionar un estado válido.";
                return RedirectToAction("Detalle", new { id });
            }

            var ingreso = await _context.Ingresos
                .Include(i => i.Vehiculo!)
                    .ThenInclude(v => v.Cliente)
                .FirstOrDefaultAsync(i => i.Id == id);
            
            if (ingreso == null)
            {
                return NotFound();
            }

            string estadoAnterior = ingreso.Estado ?? "Sin estado";
            ingreso.Estado = estado;
            
            if (estado == "Entregado" && !ingreso.FechaEgreso.HasValue)
            {
                ingreso.FechaEgreso = DateTime.Now;
            }
            
            await _context.SaveChangesAsync();

            // PATRÓN OBSERVER: Notificar cambio de estado
            var eventData = new IngresoStateChangedEventData
            {
                IngresoId = ingreso.Id,
                EstadoAnterior = estadoAnterior,
                NuevoEstado = estado,
                Patente = ingreso.Vehiculo?.Patente ?? "N/A",
                ClienteEmail = ingreso.Vehiculo?.Cliente?.Email,
                FechaCambio = DateTime.Now
            };

            await _eventSubject.NotifyAsync("IngresoStateChanged", eventData);

            // AUDITORÍA
            var rolCambio = User.IsInRole("Administrador") ? "Administrador" : User.IsInRole("Recepcionista") ? "Recepcionista" : "Mecanico";
            await _auditService.RegistrarAsync(User.Identity?.Name ?? "desconocido", rolCambio, "Ingreso", "CambioEstado",
                $"Estado de ingreso #{ingreso.Id} cambiado: {estadoAnterior} → {estado} (Vehículo: {ingreso.Vehiculo?.Patente ?? "N/A"})",
                ingreso.Id, ingreso.Vehiculo?.Patente, HttpContext.Connection.RemoteIpAddress?.ToString());

            TempData["SuccessMessage"] = "Estado actualizado correctamente.";
            return RedirectToAction("Detalle", new { id });
        }

        public async Task<IActionResult> Index(int page = 1, int pageSize = 20)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var query = _context.Ingresos
                .AsNoTracking()
                .Include(i => i.Vehiculo!)
                .ThenInclude(v => v.Cliente)
                .OrderByDescending(i => i.FechaIngreso);

            var total = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.Total = total;

            return View(items);
        }

        public async Task<IActionResult> EditarDesdeConfirmacion(int VehiculoId, string Diagnostico, int? ServicioFijoId, string? FotoTempPath)
        {
            var ingreso = new Ingreso
            {
                VehiculoId = VehiculoId,
                Diagnostico = Diagnostico,
                ServicioFijoId = ServicioFijoId
            };

            ViewBag.ServiciosFijos = await _context.ServiciosFijos.ToListAsync();
            ViewBag.FotoTempPath = FotoTempPath;

            return View("Create", ingreso);
        }
    }
}