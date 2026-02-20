using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoSys.Data;
using AutoSys.Models;
using AutoSys.Filters;
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

        public ClientesController(AutoSysDbContext context, ILogger<ClientesController> logger)
        {
            _context = context;
            _logger = logger;
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
                    TempData["SuccessMessage"] = $"Cliente {cliente.Nombre} {cliente.Apellido} creado exitosamente.";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error al crear cliente: {Message}", ex.Message);
                TempData["ErrorMessage"] = "Error al guardar el cliente. Por favor, verifique los datos e intente nuevamente.";
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
    }
}