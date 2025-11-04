using System.Diagnostics;
using AutoSys.Models;
using AutoSys.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace AutoSys.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AutoSysDbContext _context;

        public HomeController(ILogger<HomeController> logger, AutoSysDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()   
        {
            try
            {
                ViewBag.TotalClientes = await _context.Clientes.CountAsync();
                ViewBag.TotalVehiculos = await _context.Vehiculos.CountAsync();
                ViewBag.IngresosActivos = await _context.Ingresos.CountAsync(i => i.Estado != "Entregado");
                ViewBag.StockCritico = await _context.Stock.CountAsync(s => s.Cantidad <= s.StockMinimo);
                
                var ingresosRecientes = await _context.Ingresos
                    .Include(i => i.Vehiculo)
                        .ThenInclude(v => v!.Cliente)
                    .OrderByDescending(i => i.FechaIngreso)
                    .Take(5)
                    .ToListAsync();
                
                ViewBag.IngresosRecientes = ingresosRecientes;
                
                ViewBag.EnRevision = await _context.Ingresos.CountAsync(i => i.Estado == "En revisión");
                ViewBag.EnProceso = await _context.Ingresos.CountAsync(i => i.Estado == "En proceso");
                ViewBag.EnReparacion = await _context.Ingresos.CountAsync(i => i.Estado == "En reparación");
                ViewBag.Finalizados = await _context.Ingresos.CountAsync(i => i.Estado == "Finalizado");
                
                await PrepararDatosGraficosAsync();
                
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

        private async Task PrepararDatosGraficosAsync()
        {
            try
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

                ViewBag.ReparacionesData = new List<int>
                {
                    ViewBag.Finalizados ?? 0,
                    ViewBag.EnProceso ?? 0 + ViewBag.EnReparacion ?? 0,
                    ViewBag.EnRevision ?? 0
                };
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

        
        public IActionResult Privacy()
        {
            return View();
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
