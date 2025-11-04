using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoSys.Data;
using AutoSys.Models;
using System.Linq;

namespace AutoSys.Controllers
{
    [Authorize(Roles = "Administrador,Recepcionista")]
    public class VehiculosController : Controller
    {
        private readonly AutoSysDbContext _context;
        private readonly ILogger<VehiculosController> _logger;

        public VehiculosController(AutoSysDbContext context, ILogger<VehiculosController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var vehiculos = await _context.Vehiculos
                .Include(v => v.Cliente)
                .OrderBy(v => v.Patente)
                .ToListAsync();
            return View(vehiculos);
        }

        public IActionResult Create()
        {
            ViewBag.Clientes = _context.Clientes
                .OrderBy(c => c.Apellido)
                .ThenBy(c => c.Nombre)
                .ToList();
            
            ViewData["Breadcrumb"] = "Nuevo Vehículo";
            ViewData["BreadcrumbParent"] = "Vehículos";
            ViewData["BreadcrumbParentUrl"] = Url.Action("Index", "Vehiculos");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ClienteId,Patente,Marca,Modelo")] Vehiculo vehiculo)
        {
            _logger.LogInformation("POST Create - Recibido: ClienteId={ClienteId}, Patente={Patente}, Marca={Marca}, Modelo={Modelo}", 
                vehiculo.ClienteId, vehiculo.Patente, vehiculo.Marca, vehiculo.Modelo);
            
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState NO es válido:");
                foreach (var error in ModelState)
                {
                    foreach (var err in error.Value.Errors)
                    {
                        _logger.LogWarning("  - {Key}: {ErrorMessage}", error.Key, err.ErrorMessage);
                    }
                }
            }
            
            try
            {
                if (ModelState.IsValid)
                {
                    var existePatente = await _context.Vehiculos
                        .AnyAsync(v => v.Patente == vehiculo.Patente);
                    
                    if (existePatente)
                    {
                        _logger.LogWarning("Patente duplicada: {Patente}", vehiculo.Patente);
                        ModelState.AddModelError("Patente", "Ya existe un vehículo con esta patente.");
                    }
                    else
                    {
                        _context.Add(vehiculo);
                        await _context.SaveChangesAsync();
                        
                        _logger.LogInformation("✅ Vehículo creado exitosamente: {Patente}", vehiculo.Patente);
                        TempData["SuccessMessage"] = $"Vehículo {vehiculo.Patente} creado exitosamente.";
                        return RedirectToAction(nameof(Index));
                    }
                }
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error al crear vehículo: {Message}", ex.Message);
                TempData["ErrorMessage"] = "Error al guardar el vehículo. Por favor, verifique los datos e intente nuevamente.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al crear vehículo: {Message}", ex.Message);
                TempData["ErrorMessage"] = "Ocurrió un error inesperado. Por favor, contacte al administrador.";
            }

            ViewBag.Clientes = _context.Clientes
                .OrderBy(c => c.Apellido)
                .ThenBy(c => c.Nombre)
                .ToList();
            
            ViewData["Breadcrumb"] = "Nuevo Vehículo";
            ViewData["BreadcrumbParent"] = "Vehículos";
            ViewData["BreadcrumbParentUrl"] = Url.Action("Index", "Vehiculos");
            return View(vehiculo);
        }

        public async Task<IActionResult> ReparacionesDelVehiculo(int id)
        {
            var vehiculo = await _context.Vehiculos
                .Include(v => v.Cliente)
                .Include(v => v.Ingresos)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (vehiculo == null)
            {
                TempData["ErrorMessage"] = "Vehículo no encontrado.";
                return RedirectToAction("Index");
            }

            return View(vehiculo);
        }
    }
}