using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoSys.Data;
using AutoSys.Models;
using AutoSys.Filters;
using AutoSys.Services;
using System.Linq;

namespace AutoSys.Controllers
{
    [Authorize(Roles = "Administrador,Recepcionista,Mecanico")]
    [RequirePermiso("VerVehiculos")]
    public class VehiculosController : Controller
    {
        private readonly AutoSysDbContext _context;
        private readonly ILogger<VehiculosController> _logger;
        private readonly IAuditService _auditService;

        public VehiculosController(AutoSysDbContext context, ILogger<VehiculosController> logger, IAuditService auditService)
        {
            _context = context;
            _logger = logger;
            _auditService = auditService;
        }

        public async Task<IActionResult> Index()
        {
            var vehiculos = await _context.Vehiculos
                .Include(v => v.Cliente)
                .Include(v => v.Ingresos)
                .OrderBy(v => v.Patente)
                .ToListAsync();
            return View(vehiculos);
        }

        [RequirePermiso("CrearVehiculos")]
        public IActionResult Create()
        {
            var clientDataCreate = _context.Clientes
                .OrderBy(c => c.Apellido)
                .ThenBy(c => c.Nombre)
                .Select(c => new { 
                    Id = c.Id, 
                    Nombre = c.Nombre, 
                    Apellido = c.Apellido, 
                    DNI = c.DNI 
                })
                .ToList();
            ViewBag.Clientes = clientDataCreate;
            ViewBag.ClientesJson = System.Text.Json.JsonSerializer.Serialize(clientDataCreate);
            
            ViewData["Breadcrumb"] = "Nuevo Vehículo";
            ViewData["BreadcrumbParent"] = "Vehículos";
            ViewData["BreadcrumbParentUrl"] = Url.Action("Index", "Vehiculos");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermiso("CrearVehiculos")]
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
                        
                        // Crear primer registro de historial
                        var historial = new HistorialPropietario
                        {
                            VehiculoId = vehiculo.Id,
                            ClienteId = vehiculo.ClienteId,
                            FechaDesde = DateTime.Now,
                            EsPropietarioActual = true
                        };
                        _context.HistorialesPropietarios.Add(historial);
                        await _context.SaveChangesAsync();
                        
                        _logger.LogInformation("✅ Vehículo creado exitosamente: {Patente}", vehiculo.Patente);

                        // AUDITORÍA
                        var rolActual = User.IsInRole("Administrador") ? "Administrador" : User.IsInRole("Recepcionista") ? "Recepcionista" : "Mecanico";
                        await _auditService.RegistrarAsync(User.Identity?.Name ?? "desconocido", rolActual, "Vehiculo", "Crear",
                            $"Vehículo creado: {vehiculo.Patente} ({vehiculo.Marca} {vehiculo.Modelo})",
                            vehiculo.Id, vehiculo.Patente, HttpContext.Connection.RemoteIpAddress?.ToString());

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

            var clientDataCreateFail = _context.Clientes
                .OrderBy(c => c.Apellido)
                .ThenBy(c => c.Nombre)
                .Select(c => new { 
                    Id = c.Id, 
                    Nombre = c.Nombre, 
                    Apellido = c.Apellido, 
                    DNI = c.DNI 
                })
                .ToList();
            ViewBag.Clientes = clientDataCreateFail;
            ViewBag.ClientesJson = System.Text.Json.JsonSerializer.Serialize(clientDataCreateFail);
            
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

        // ── EDITAR ────────────────────────────────────────────

        [RequirePermiso("EditarVehiculos")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var vehiculo = await _context.Vehiculos
                .Include(v => v.Cliente)
                .FirstOrDefaultAsync(v => v.Id == id);
            if (vehiculo == null) return NotFound();

            var clientData = _context.Clientes
                .OrderBy(c => c.Apellido)
                .ThenBy(c => c.Nombre)
                .Select(c => new { 
                    Id = c.Id, 
                    Nombre = c.Nombre, 
                    Apellido = c.Apellido, 
                    DNI = c.DNI 
                })
                .ToList();
            ViewBag.Clientes = clientData;
            ViewBag.ClientesJson = System.Text.Json.JsonSerializer.Serialize(clientData);

            return View(vehiculo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermiso("EditarVehiculos")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ClienteId,Patente,Marca,Modelo")] Vehiculo vehiculo)
        {
            if (id != vehiculo.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Verificar patente duplicada (excluyendo el vehículo actual)
                    var existePatente = await _context.Vehiculos
                        .AnyAsync(v => v.Patente == vehiculo.Patente && v.Id != vehiculo.Id);

                    if (existePatente)
                    {
                        _logger.LogWarning("Patente duplicada al editar: {Patente}", vehiculo.Patente);
                        TempData["ErrorMessage"] = $"Ya existe otro vehículo con la patente {vehiculo.Patente}.";
                    }
                    else
                    {
                        _context.Update(vehiculo);
                        await _context.SaveChangesAsync();
                        _logger.LogInformation("Vehículo editado exitosamente: {Id} - {Patente}", vehiculo.Id, vehiculo.Patente);

                        // AUDITORÍA
                        var rolEdit = User.IsInRole("Administrador") ? "Administrador" : User.IsInRole("Recepcionista") ? "Recepcionista" : "Mecanico";
                        await _auditService.RegistrarAsync(User.Identity?.Name ?? "desconocido", rolEdit, "Vehiculo", "Editar",
                            $"Vehículo editado: {vehiculo.Patente} ({vehiculo.Marca} {vehiculo.Modelo})",
                            vehiculo.Id, vehiculo.Patente, HttpContext.Connection.RemoteIpAddress?.ToString());

                        TempData["SuccessMessage"] = $"Vehículo {vehiculo.Patente} actualizado correctamente.";
                        return RedirectToAction(nameof(Index));
                    }
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!VehiculoExists(vehiculo.Id))
                    {
                        _logger.LogWarning("Vehículo no encontrado al editar: {Id}", id);
                        return NotFound();
                    }
                    else
                    {
                        _logger.LogError(ex, "Error de concurrencia al editar vehículo {Id}", id);
                        TempData["ErrorMessage"] = "Error al actualizar. El vehículo pudo haber sido modificado por otro usuario.";
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al editar vehículo {Id}", id);
                    TempData["ErrorMessage"] = "Error al actualizar el vehículo.";
                }
            }

            var clientDataEditFail = _context.Clientes
                .OrderBy(c => c.Apellido)
                .ThenBy(c => c.Nombre)
                .Select(c => new { 
                    Id = c.Id, 
                    Nombre = c.Nombre, 
                    Apellido = c.Apellido, 
                    DNI = c.DNI 
                })
                .ToList();
            ViewBag.Clientes = clientDataEditFail;
            ViewBag.ClientesJson = System.Text.Json.JsonSerializer.Serialize(clientDataEditFail);

            return View(vehiculo);
        }

        // ── ELIMINAR ──────────────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                _logger.LogInformation("Iniciando eliminación de vehículo. Id: {Id}", id);

                var vehiculo = await _context.Vehiculos
                    .Include(v => v.Ingresos)
                    .FirstOrDefaultAsync(v => v.Id == id);

                if (vehiculo == null)
                {
                    _logger.LogWarning("Vehículo no encontrado para eliminar. Id: {Id}", id);
                    TempData["ErrorMessage"] = "El vehículo no fue encontrado.";
                    return RedirectToAction(nameof(Index));
                }

                if (vehiculo.Ingresos.Any())
                {
                    _logger.LogWarning("No se puede eliminar vehículo {Id} porque tiene {Count} ingresos asociados", 
                        id, vehiculo.Ingresos.Count);
                    TempData["ErrorMessage"] = $"No se puede eliminar el vehículo {vehiculo.Patente} porque tiene {vehiculo.Ingresos.Count} ingreso(s) asociado(s).";
                    return RedirectToAction(nameof(Index));
                }

                _context.Vehiculos.Remove(vehiculo);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Vehículo eliminado exitosamente. Id: {Id}, Patente: {Patente}", id, vehiculo.Patente);

                // AUDITORÍA
                var rolDel = User.IsInRole("Administrador") ? "Administrador" : User.IsInRole("Recepcionista") ? "Recepcionista" : "Mecanico";
                await _auditService.RegistrarAsync(User.Identity?.Name ?? "desconocido", rolDel, "Vehiculo", "Eliminar",
                    $"Vehículo eliminado: {vehiculo.Patente} ({vehiculo.Marca} {vehiculo.Modelo})",
                    id, vehiculo.Patente, HttpContext.Connection.RemoteIpAddress?.ToString());

                TempData["SuccessMessage"] = $"Vehículo {vehiculo.Patente} eliminado correctamente.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar vehículo. Id: {Id}", id);
                TempData["ErrorMessage"] = "Error al eliminar el vehículo. Por favor, intente nuevamente.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermiso("TransferirVehiculo")]
        public async Task<IActionResult> TransferirVehiculo(int VehiculoId, int NuevoClienteId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var vehiculo = await _context.Vehiculos
                    .Include(v => v.Cliente)
                    .FirstOrDefaultAsync(v => v.Id == VehiculoId);

                if (vehiculo == null) return NotFound();

                var nuevoCliente = await _context.Clientes.FindAsync(NuevoClienteId);
                if (nuevoCliente == null)
                {
                    TempData["ErrorMessage"] = "El nuevo cliente seleccionado no es válido.";
                    return RedirectToAction("Edit", new { id = VehiculoId });
                }

                if (vehiculo.ClienteId == NuevoClienteId)
                {
                    TempData["ErrorMessage"] = "El vehículo ya pertenece a ese cliente.";
                    return RedirectToAction("Edit", new { id = VehiculoId });
                }

                // 1. Cerrar historial actual
                var historialActual = await _context.HistorialesPropietarios
                    .FirstOrDefaultAsync(h => h.VehiculoId == VehiculoId && h.EsPropietarioActual);

                if (historialActual != null)
                {
                    historialActual.FechaHasta = DateTime.Now;
                    historialActual.EsPropietarioActual = false;
                    _context.Update(historialActual);
                }

                // 2. Crear nuevo historial
                var nuevoHistorial = new HistorialPropietario
                {
                    VehiculoId = VehiculoId,
                    ClienteId = NuevoClienteId,
                    FechaDesde = DateTime.Now,
                    EsPropietarioActual = true
                };
                _context.HistorialesPropietarios.Add(nuevoHistorial);

                // 3. Actualizar dueño actual en Vehiculo
                string duenoAnterior = $"{vehiculo.Cliente?.Nombre} {vehiculo.Cliente?.Apellido}";
                vehiculo.ClienteId = NuevoClienteId;
                _context.Update(vehiculo);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // AUDITORÍA
                var rol = User.IsInRole("Administrador") ? "Administrador" : User.IsInRole("Recepcionista") ? "Recepcionista" : "Mecanico";
                await _auditService.RegistrarAsync(User.Identity?.Name ?? "desconocido", rol, "Vehiculo", "Transferir",
                    $"Vehículo {vehiculo.Patente} transferido de {duenoAnterior} a {nuevoCliente.Nombre} {nuevoCliente.Apellido}",
                    vehiculo.Id, vehiculo.Patente, HttpContext.Connection.RemoteIpAddress?.ToString());

                TempData["SuccessMessage"] = $"Vehículo transferido exitosamente a {nuevoCliente.Nombre} {nuevoCliente.Apellido}.";
                return RedirectToAction("Details", new { id = VehiculoId });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error al transferir vehículo {Id}", VehiculoId);
                TempData["ErrorMessage"] = "Ocurrió un error al procesar la transferencia.";
                return RedirectToAction("Edit", new { id = VehiculoId });
            }
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var vehiculo = await _context.Vehiculos
                .Include(v => v.Cliente)
                .Include(v => v.HistorialPropietarios.OrderByDescending(h => h.FechaDesde))
                    .ThenInclude(h => h.Cliente)
                .Include(v => v.Ingresos.OrderByDescending(i => i.FechaIngreso))
                    .ThenInclude(i => i.Cliente)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (vehiculo == null) return NotFound();

            return View(vehiculo);
        }

        private bool VehiculoExists(int id)
        {
            return _context.Vehiculos.Any(e => e.Id == id);
        }
    }
}