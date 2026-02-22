using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoSys.Data;
using AutoSys.Models;
using AutoSys.Filters;
using AutoSys.Services;
using System.Linq;
using Microsoft.Data.SqlClient;

namespace AutoSys.Controllers
{
    [Authorize(Roles = "Administrador,Recepcionista,Mecanico")]
    [RequirePermiso("VerClientes")]
    public class ClientesController : Controller
    {
        private readonly AutoSysDbContext _context;
        private readonly ILogger<ClientesController> _logger;
        private readonly IAuditService _auditService;

        public ClientesController(AutoSysDbContext context, ILogger<ClientesController> logger, IAuditService auditService)
        {
            _context = context;
            _logger = logger;
            _auditService = auditService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var clientes = await _context.Clientes
                    .Include(c => c.Vehiculos)
                    .OrderBy(c => c.Apellido)
                    .ThenBy(c => c.Nombre)
                    .ToListAsync();
                
                ViewData["Breadcrumb"] = "Clientes";
                return View(clientes);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error de base de datos al obtener clientes: {Message}", ex.Message);
                TempData["ErrorMessage"] = "Error al cargar la lista de clientes. Por favor, intente nuevamente.";
                return View(new List<Cliente>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al obtener clientes: {Message}", ex.Message);
                TempData["ErrorMessage"] = "Ocurrió un error inesperado. Por favor, contacte al administrador.";
                return View(new List<Cliente>());
            }
        }

        [RequirePermiso("CrearClientes")]
        public IActionResult Create()
        {
            ViewData["Breadcrumb"] = "Nuevo Cliente";
            ViewData["BreadcrumbParent"] = "Clientes";
            ViewData["BreadcrumbParentUrl"] = Url.Action("Index", "Clientes");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermiso("CrearClientes")]
        public async Task<IActionResult> Create([Bind("Nombre,Apellido,DNI,Telefono,Email")] Cliente cliente)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    cliente.FechaRegistro = DateTime.Now;
                    _context.Add(cliente);
                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation("Cliente creado exitosamente: {ClienteNombre} {ClienteApellido}", cliente.Nombre, cliente.Apellido);

                    // AUDITORÍA
                    var rolActual = User.IsInRole("Administrador") ? "Administrador" : User.IsInRole("Recepcionista") ? "Recepcionista" : "Mecanico";
                    await _auditService.RegistrarAsync(User.Identity?.Name ?? "desconocido", rolActual, "Cliente", "Crear",
                        $"Cliente creado: {cliente.Nombre} {cliente.Apellido} (DNI: {cliente.DNI})",
                        cliente.Id, $"{cliente.Nombre} {cliente.Apellido}", HttpContext.Connection.RemoteIpAddress?.ToString());

                    TempData["SuccessMessage"] = $"Cliente {cliente.Nombre} {cliente.Apellido} creado exitosamente.";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error al crear cliente: {Message}", ex.InnerException?.Message ?? ex.Message);
                if (ex.InnerException?.Message?.Contains("IX_Clientes_Email") == true
                    || ex.InnerException?.Message?.Contains("duplicate key") == true)
                {
                    TempData["ErrorMessage"] = $"Ya existe un cliente registrado con el email '{cliente.Email}'. Por favor, use un email diferente.";
                }
                else if (ex.InnerException?.Message?.Contains("IX_Clientes_DNI") == true)
                {
                    TempData["ErrorMessage"] = $"Ya existe un cliente registrado con el DNI '{cliente.DNI}'. Por favor, verifique el DNI.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Error al guardar el cliente. Por favor, verifique los datos e intente nuevamente.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al crear cliente: {Message}", ex.Message);
                TempData["ErrorMessage"] = "Ocurrió un error inesperado. Por favor, contacte al administrador.";
            }

            ViewData["Breadcrumb"] = "Nuevo Cliente";
            ViewData["BreadcrumbParent"] = "Clientes";
            ViewData["BreadcrumbParentUrl"] = Url.Action("Index", "Clientes");
            return View(cliente);
        }

        public async Task<IActionResult> VehiculosDelCliente(int id)
        {
            try
            {
                var cliente = await _context.Clientes
                    .Include(c => c.Vehiculos)
                        .ThenInclude(v => v.Ingresos)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (cliente == null)
                {
                    _logger.LogWarning("Intento de acceso a cliente inexistente. ID: {ClienteId}", id);
                    TempData["ErrorMessage"] = "Cliente no encontrado.";
                    return RedirectToAction("Index");
                }

                ViewData["Breadcrumb"] = $"{cliente.Nombre} {cliente.Apellido}";
                ViewData["BreadcrumbParent"] = "Clientes";
                ViewData["BreadcrumbParentUrl"] = Url.Action("Index", "Clientes");
                
                return View(cliente);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error de base de datos al obtener detalles del cliente {ClienteId}: {Message}", id, ex.Message);
                TempData["ErrorMessage"] = "Error al cargar los detalles del cliente. Por favor, intente nuevamente.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al obtener detalles del cliente {ClienteId}: {Message}", id, ex.Message);
                TempData["ErrorMessage"] = "Ocurrió un error inesperado. Por favor, contacte al administrador.";
                return RedirectToAction("Index");
            }
        }

        // ── EDITAR ────────────────────────────────────────────

        [RequirePermiso("EditarClientes")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null) return NotFound();

            ViewData["Breadcrumb"] = "Editar Cliente";
            ViewData["BreadcrumbParent"] = "Clientes";
            ViewData["BreadcrumbParentUrl"] = Url.Action("Index", "Clientes");
            return View(cliente);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermiso("EditarClientes")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Apellido,DNI,Telefono,Email,FechaRegistro")] Cliente cliente)
        {
            if (id != cliente.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Verificar DNI duplicado (excluyendo el cliente actual)
                    if (!string.IsNullOrEmpty(cliente.DNI))
                    {
                        var existeDni = await _context.Clientes
                            .AnyAsync(c => c.DNI == cliente.DNI && c.Id != cliente.Id);
                        if (existeDni)
                        {
                            _logger.LogWarning("DNI duplicado al editar cliente: {DNI}", cliente.DNI);
                            TempData["ErrorMessage"] = $"Ya existe otro cliente con el DNI '{cliente.DNI}'.";
                            ViewData["Breadcrumb"] = "Editar Cliente";
                            ViewData["BreadcrumbParent"] = "Clientes";
                            ViewData["BreadcrumbParentUrl"] = Url.Action("Index", "Clientes");
                            return View(cliente);
                        }
                    }

                    // Verificar Email duplicado (excluyendo el cliente actual)
                    if (!string.IsNullOrEmpty(cliente.Email))
                    {
                        var existeEmail = await _context.Clientes
                            .AnyAsync(c => c.Email == cliente.Email && c.Id != cliente.Id);
                        if (existeEmail)
                        {
                            _logger.LogWarning("Email duplicado al editar cliente: {Email}", cliente.Email);
                            TempData["ErrorMessage"] = $"Ya existe otro cliente con el email '{cliente.Email}'.";
                            ViewData["Breadcrumb"] = "Editar Cliente";
                            ViewData["BreadcrumbParent"] = "Clientes";
                            ViewData["BreadcrumbParentUrl"] = Url.Action("Index", "Clientes");
                            return View(cliente);
                        }
                    }

                    _context.Update(cliente);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Cliente editado exitosamente: {Id} - {Nombre} {Apellido}", 
                        cliente.Id, cliente.Nombre, cliente.Apellido);

                    // AUDITORÍA
                    var rolEdit = User.IsInRole("Administrador") ? "Administrador" : User.IsInRole("Recepcionista") ? "Recepcionista" : "Mecanico";
                    await _auditService.RegistrarAsync(User.Identity?.Name ?? "desconocido", rolEdit, "Cliente", "Editar",
                        $"Cliente editado: {cliente.Nombre} {cliente.Apellido} (DNI: {cliente.DNI})",
                        cliente.Id, $"{cliente.Nombre} {cliente.Apellido}", HttpContext.Connection.RemoteIpAddress?.ToString());

                    TempData["SuccessMessage"] = $"Cliente {cliente.Nombre} {cliente.Apellido} actualizado correctamente.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!ClienteExists(cliente.Id))
                    {
                        _logger.LogWarning("Cliente no encontrado al editar: {Id}", id);
                        return NotFound();
                    }
                    else
                    {
                        _logger.LogError(ex, "Error de concurrencia al editar cliente {Id}", id);
                        TempData["ErrorMessage"] = "Error al actualizar. El cliente pudo haber sido modificado por otro usuario.";
                    }
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Error al editar cliente: {Message}", ex.InnerException?.Message ?? ex.Message);
                    if (ex.InnerException?.Message?.Contains("IX_Clientes_Email") == true
                        || ex.InnerException?.Message?.Contains("duplicate key") == true)
                    {
                        TempData["ErrorMessage"] = $"Ya existe un cliente con el email '{cliente.Email}'.";
                    }
                    else if (ex.InnerException?.Message?.Contains("IX_Clientes_DNI") == true)
                    {
                        TempData["ErrorMessage"] = $"Ya existe un cliente con el DNI '{cliente.DNI}'.";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Error al actualizar el cliente. Verifique los datos e intente nuevamente.";
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error inesperado al editar cliente {Id}", id);
                    TempData["ErrorMessage"] = "Ocurrió un error inesperado. Por favor, contacte al administrador.";
                }
            }

            ViewData["Breadcrumb"] = "Editar Cliente";
            ViewData["BreadcrumbParent"] = "Clientes";
            ViewData["BreadcrumbParentUrl"] = Url.Action("Index", "Clientes");
            return View(cliente);
        }

        private bool ClienteExists(int id)
        {
            return _context.Clientes.Any(e => e.Id == id);
        }
    }
}