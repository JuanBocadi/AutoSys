using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoSys.Data;
using AutoSys.Models;

namespace AutoSys.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class ReportesController : Controller
    {
        private readonly AutoSysDbContext _context;

        public ReportesController(AutoSysDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.TotalClientes = await _context.Clientes.CountAsync();
            ViewBag.TotalVehiculos = await _context.Vehiculos.CountAsync();
            ViewBag.TotalIngresos = await _context.Ingresos.CountAsync();
            ViewBag.IngresosEnTaller = await _context.Ingresos.CountAsync(i => !i.FechaEgreso.HasValue);

            var facturas = await _context.Facturas.ToListAsync();
            ViewBag.TotalFacturas = facturas.Count;
            ViewBag.FacturasPendientes = facturas.Count(f => f.Estado == "Pendiente");
            ViewBag.TotalRecaudado = facturas.Where(f => f.Estado == "Pagada").Sum(f => f.Total);
            ViewBag.PromedioFactura = facturas.Any() ? facturas.Average(f => f.Total) : 0;

            ViewBag.StockCritico = await _context.Stock.CountAsync(s => s.Cantidad < s.StockMinimo);

            var primerDiaMes = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            ViewBag.IngresosMesActual = await _context.Ingresos
                .CountAsync(i => i.FechaIngreso >= primerDiaMes);

            return View();
        }

        public async Task<IActionResult> IngresosPorPeriodo(DateTime? desde, DateTime? hasta)
        {
            desde ??= DateTime.Now.AddMonths(-1);
            hasta ??= DateTime.Now;

            var ingresos = await _context.Ingresos
                .Include(i => i.Vehiculo!)
                    .ThenInclude(v => v.Cliente)
                .Where(i => i.FechaIngreso >= desde && i.FechaIngreso <= hasta)
                .OrderByDescending(i => i.FechaIngreso)
                .ToListAsync();

            ViewBag.Desde = desde;
            ViewBag.Hasta = hasta;
            ViewBag.TotalIngresos = ingresos.Count;
            ViewBag.EnProceso = ingresos.Count(i => !i.FechaEgreso.HasValue);
            ViewBag.Finalizados = ingresos.Count(i => i.FechaEgreso.HasValue);

            return View(ingresos);
        }

        // Reporte de facturación por período
        public async Task<IActionResult> FacturacionPorPeriodo(DateTime? desde, DateTime? hasta)
        {
            desde ??= DateTime.Now.AddMonths(-1);
            hasta ??= DateTime.Now;

            var facturas = await _context.Facturas
                .Include(f => f.Cliente)
                .Include(f => f.Detalles)
                .Where(f => f.FechaEmision >= desde && f.FechaEmision <= hasta)
                .OrderByDescending(f => f.FechaEmision)
                .ToListAsync();

            ViewBag.Desde = desde;
            ViewBag.Hasta = hasta;
            ViewBag.TotalFacturas = facturas.Count;
            ViewBag.TotalPagadas = facturas.Count(f => f.Estado == "Pagada");
            ViewBag.TotalRecaudado = facturas.Where(f => f.Estado == "Pagada").Sum(f => f.Total);
            ViewBag.TotalPendiente = facturas.Where(f => f.Estado == "Pendiente").Sum(f => f.Total);

            return View(facturas);
        }

        public async Task<IActionResult> ClientesActivos()
        {
            var clientes = await _context.Clientes
                .Include(c => c.Vehiculos)
                    .ThenInclude(v => v.Ingresos)
                .ToListAsync();

            var clientesConEstadisticas = clientes.Select(c => new
            {
                Cliente = c,
                TotalIngresos = c.Vehiculos.Sum(v => v.Ingresos.Count),
                TotalVehiculos = c.Vehiculos.Count,
                UltimoIngreso = c.Vehiculos.SelectMany(v => v.Ingresos)
                    .OrderByDescending(i => i.FechaIngreso)
                    .FirstOrDefault()?.FechaIngreso
            })
            .OrderByDescending(x => x.TotalIngresos)
            .Take(20)
            .ToList();

            ViewBag.ClientesConEstadisticas = clientesConEstadisticas;
            return View();
        }

        public async Task<IActionResult> StockBajo()
        {
            var items = await _context.Stock
                .Where(s => s.Cantidad <= s.StockMinimo * 2)
                .OrderBy(s => s.Cantidad)
                .ToListAsync();

            ViewBag.ItemsCriticos = items.Count(s => s.Cantidad < s.StockMinimo);
            ViewBag.ItemsBajo = items.Count(s => s.Cantidad >= s.StockMinimo && s.Cantidad <= s.StockMinimo * 2);

            return View(items);
        }

        public async Task<IActionResult> TiemposReparacion()
        {
            var ingresosFinalizados = await _context.Ingresos
                .Include(i => i.Vehiculo!)
                    .ThenInclude(v => v.Cliente)
                .Where(i => i.FechaEgreso.HasValue)
                .OrderByDescending(i => i.FechaEgreso)
                .Take(100)
                .ToListAsync();

            if (ingresosFinalizados.Any())
            {
                var tiempos = ingresosFinalizados
                    .Select(i => (i.FechaEgreso!.Value - i.FechaIngreso).TotalDays)
                    .ToList();

                ViewBag.TiempoPromedio = tiempos.Average();
                ViewBag.TiempoMinimo = tiempos.Min();
                ViewBag.TiempoMaximo = tiempos.Max();
            }
            else
            {
                ViewBag.TiempoPromedio = 0;
                ViewBag.TiempoMinimo = 0;
                ViewBag.TiempoMaximo = 0;
            }

            return View(ingresosFinalizados);
        }
    }
}