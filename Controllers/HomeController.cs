using System.Diagnostics;
using AutoSys.Models;
using AutoSys.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

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

        public IActionResult Index()   
        {
            // Estadísticas del dashboard
            ViewBag.TotalClientes = _context.Clientes.Count();
            ViewBag.TotalVehiculos = _context.Vehiculos.Count();
            ViewBag.IngresosActivos = _context.Ingresos.Count(i => i.Estado != "Entregado");
            ViewBag.StockCritico = _context.Stock.Count(s => s.Cantidad <= s.StockMinimo);
            
            // Ingresos recientes
            var ingresosRecientes = _context.Ingresos
                .Include(i => i.Vehiculo)
                    .ThenInclude(v => v!.Cliente)
                .OrderByDescending(i => i.FechaIngreso)
                .Take(5)
                .ToList();
            
            ViewBag.IngresosRecientes = ingresosRecientes;
            
            // Estadísticas por estado
            ViewBag.EnRevision = _context.Ingresos.Count(i => i.Estado == "En revisión");
            ViewBag.EnProceso = _context.Ingresos.Count(i => i.Estado == "En proceso");
            ViewBag.EnReparacion = _context.Ingresos.Count(i => i.Estado == "En reparación");
            ViewBag.Finalizados = _context.Ingresos.Count(i => i.Estado == "Finalizado");
            
            return View();
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
