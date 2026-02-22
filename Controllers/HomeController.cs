using System.Diagnostics;
using System.Security.Claims;
using AutoSys.Models;
using AutoSys.Data;
using AutoSys.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace AutoSys.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AutoSysDbContext _context;
        private readonly IPermissionService _permissionService;
        private readonly UserManager<IdentityUser> _userManager;

        public HomeController(
            ILogger<HomeController> logger,
            AutoSysDbContext context,
            IPermissionService permissionService,
            UserManager<IdentityUser> userManager)
        {
            _logger = logger;
            _context = context;
            _permissionService = permissionService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()   
        {
            try
            {
                // Obtener permisos efectivos del usuario actual
                var permisos = await ObtenerPermisosUsuarioAsync();

                // Pasar flags de permisos a la vista
                ViewBag.PuedeVerClientes = permisos.VerClientes;
                ViewBag.PuedeVerVehiculos = permisos.VerVehiculos;
                ViewBag.PuedeVerIngresos = permisos.VerIngresos;
                ViewBag.PuedeVerStock = permisos.VerStock;
                ViewBag.PuedeVerFacturacion = permisos.VerFacturacion;
                ViewBag.PuedeVerReparaciones = permisos.VerReparaciones;
                ViewBag.PuedeVerReportes = permisos.VerReportes;
                ViewBag.NombreUsuario = User.Identity?.Name ?? "Usuario";
                ViewBag.RolUsuario = await ObtenerRolUsuarioAsync();

                // Solo cargar datos que el usuario tiene permiso de ver
                if (permisos.VerClientes)
                    ViewBag.TotalClientes = await _context.Clientes.CountAsync();
                else
                    ViewBag.TotalClientes = 0;

                if (permisos.VerVehiculos)
                    ViewBag.TotalVehiculos = await _context.Vehiculos.CountAsync();
                else
                    ViewBag.TotalVehiculos = 0;

                // Datos de ingresos (listado y conteo activos)
                if (permisos.VerIngresos)
                {
                    ViewBag.IngresosActivos = await _context.Ingresos.CountAsync(i => i.Estado != "Entregado");

                    var ingresosRecientes = await _context.Ingresos
                        .Include(i => i.Vehiculo)
                            .ThenInclude(v => v!.Cliente)
                        .OrderByDescending(i => i.FechaIngreso)
                        .Take(5)
                        .ToListAsync();
                    ViewBag.IngresosRecientes = ingresosRecientes;
                }
                else
                {
                    ViewBag.IngresosActivos = 0;
                    ViewBag.IngresosRecientes = new List<Ingreso>();
                }

                // Datos de reparaciones (conteos por estado)
                if (permisos.VerReparaciones)
                {
                    ViewBag.EnRevision = await _context.Ingresos.CountAsync(i => i.Estado == "En revisión");
                    ViewBag.EnProceso = await _context.Ingresos.CountAsync(i => i.Estado == "En proceso");
                    ViewBag.EnReparacion = await _context.Ingresos.CountAsync(i => i.Estado == "En reparación");
                    ViewBag.Finalizados = await _context.Ingresos.CountAsync(i => i.Estado == "Finalizado");
                }
                else
                {
                    ViewBag.EnRevision = 0;
                    ViewBag.EnProceso = 0;
                    ViewBag.EnReparacion = 0;
                    ViewBag.Finalizados = 0;
                }

                if (permisos.VerStock)
                    ViewBag.StockCritico = await _context.Stock.CountAsync(s => s.Cantidad <= s.StockMinimo);
                else
                    ViewBag.StockCritico = 0;

                // Gráficos: solo si tiene permisos relevantes
                await PrepararDatosGraficosAsync(permisos);
                
                return View();
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error de base de datos al cargar el dashboard: {Message}", ex.Message);
                TempData["ErrorMessage"] = "Error al conectar con la base de datos. Por favor, intente nuevamente.";
                CrearViewBagVacio();
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al cargar el dashboard: {Message}", ex.Message);
                TempData["ErrorMessage"] = "Ocurrió un error al cargar el dashboard. Por favor, contacte al administrador.";
                CrearViewBagVacio();
                return View();
            }
        }

        private async Task<UserPermission> ObtenerPermisosUsuarioAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            var rol = await ObtenerRolUsuarioAsync();
            return await _permissionService.ObtenerPermisosEfectivosAsync(userId, rol);
        }

        private async Task<string> ObtenerRolUsuarioAsync()
        {
            if (!User.Identity?.IsAuthenticated ?? true)
                return "Invitado";

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return "Invitado";

            var roles = await _userManager.GetRolesAsync(user);
            return roles.FirstOrDefault() ?? "Invitado";
        }

        private async Task PrepararDatosGraficosAsync(UserPermission permisos)
        {
            try
            {
                // Gráfico de facturación: solo si puede ver facturación
                if (permisos.VerFacturacion)
                {
                    var fechaInicio = DateTime.Now.AddMonths(-6);
                    var facturacionMensual = await _context.Facturas
                        .Where(f => f.FechaEmision >= fechaInicio)
                        .GroupBy(f => new { f.FechaEmision.Year, f.FechaEmision.Month })
                        .Select(g => new
                        {
                            Year = g.Key.Year,
                            Month = g.Key.Month,
                            Total = g.Sum(f => f.Total)
                        })
                        .OrderBy(x => x.Year).ThenBy(x => x.Month)
                        .ToListAsync();

                    ViewBag.MesesLabels = facturacionMensual
                        .Select(f => new DateTime(f.Year, f.Month, 1).ToString("MMM yyyy"))
                        .ToList();
                    ViewBag.FacturacionData = facturacionMensual.Select(f => f.Total).ToList();
                }
                else
                {
                    ViewBag.MesesLabels = new List<string>();
                    ViewBag.FacturacionData = new List<decimal>();
                }

                // Gráfico de reparaciones: solo si puede ver reparaciones
                if (permisos.VerReparaciones)
                {
                    ViewBag.ReparacionesData = new List<int>
                    {
                        ViewBag.Finalizados ?? 0,
                        (ViewBag.EnProceso ?? 0) + (ViewBag.EnReparacion ?? 0),
                        ViewBag.EnRevision ?? 0
                    };
                }
                else
                {
                    ViewBag.ReparacionesData = new List<int> { 0, 0, 0 };
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al preparar datos para gráficos");
                ViewBag.MesesLabels = new List<string>();
                ViewBag.FacturacionData = new List<decimal>();
                ViewBag.ReparacionesData = new List<int> { 0, 0, 0 };
            }
        }

        private void CrearViewBagVacio()
        {
            ViewBag.PuedeVerClientes = false;
            ViewBag.PuedeVerVehiculos = false;
            ViewBag.PuedeVerIngresos = false;
            ViewBag.PuedeVerStock = false;
            ViewBag.PuedeVerFacturacion = false;
            ViewBag.PuedeVerReparaciones = false;
            ViewBag.PuedeVerReportes = false;
            ViewBag.NombreUsuario = User.Identity?.Name ?? "Usuario";
            ViewBag.RolUsuario = "N/A";
            ViewBag.TotalClientes = 0;
            ViewBag.TotalVehiculos = 0;
            ViewBag.IngresosActivos = 0;
            ViewBag.StockCritico = 0;
            ViewBag.IngresosRecientes = new List<Ingreso>();
            ViewBag.EnRevision = 0;
            ViewBag.EnProceso = 0;
            ViewBag.EnReparacion = 0;
            ViewBag.Finalizados = 0;
            ViewBag.MesesLabels = new List<string>();
            ViewBag.FacturacionData = new List<decimal>();
            ViewBag.ReparacionesData = new List<int> { 0, 0, 0 };
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel  // Crea un modelo de error con el id para identificar errores)
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
