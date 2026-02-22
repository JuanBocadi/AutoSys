using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoSys.Data;
using AutoSys.Models;
using AutoSys.Filters;
using AutoSys.Patterns.Factory;
using AutoSys.Patterns.Singleton;
using AutoSys.Services;
using System.Text.Json;

namespace AutoSys.Controllers
{
    [Authorize(Roles = "Administrador,Recepcionista,Mecanico")]
    [RequirePermiso("VerReportes")]
    public class ReportesController : Controller
    {
        private readonly AutoSysDbContext _context;
        private readonly IPdfExportService _pdfService;

        public ReportesController(AutoSysDbContext context, IPdfExportService pdfService)
        {
            _context = context;
            _pdfService = pdfService;
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

        public IActionResult IngresosPorPeriodo(DateTime? desde, DateTime? hasta)
        {
            desde ??= DateTime.Now.AddMonths(-1);
            hasta ??= DateTime.Now;

            // PATRÓN FACTORY METHOD: Crear reporte de ingresos usando factory
            var reportFactory = new IngresosPeriodoReportFactory(_context, desde.Value, hasta.Value);
            var report = reportFactory.CreateReport();
            var data = report.GenerateData();

            ViewBag.Desde = data["Desde"];
            ViewBag.Hasta = data["Hasta"];
            ViewBag.TotalIngresos = data["TotalIngresos"];
            ViewBag.EnProceso = data["EnProceso"];
            ViewBag.Finalizados = data["Finalizados"];

            var ingresos = (List<Ingreso>)data["Ingresos"];
            return View(ingresos);
        }

        // Reporte de facturación por período
        public IActionResult FacturacionPorPeriodo(DateTime? desde, DateTime? hasta)
        {
            desde ??= DateTime.Now.AddMonths(-1);
            hasta ??= DateTime.Now;

            // PATRÓN FACTORY METHOD: Crear reporte de facturación usando factory
            var reportFactory = new FacturacionPeriodoReportFactory(_context, desde.Value, hasta.Value);
            var report = reportFactory.CreateReport();
            var data = report.GenerateData();

            ViewBag.Desde = data["Desde"];
            ViewBag.Hasta = data["Hasta"];
            ViewBag.TotalFacturas = data["TotalFacturas"];
            ViewBag.TotalPagadas = data["TotalPagadas"];
            ViewBag.TotalRecaudado = data["TotalRecaudado"];
            ViewBag.TotalPendiente = data["TotalPendiente"];

            var facturas = (List<Factura>)data["Facturas"];
            return View(facturas);
        }

        public IActionResult StockBajo()
        {
            // PATRÓN FACTORY METHOD: Crear reporte de stock bajo usando factory
            // PATRÓN SINGLETON: Obtener configuración del nivel de alerta
            var config = AppConfigurationManager.Instance;
            int warningLevel = config.GetSettingAsInt("StockWarningLevel", 10);

            var reportFactory = new StockBajoReportFactory(_context);
            var report = reportFactory.CreateReport();
            var data = report.GenerateData();

            ViewBag.TotalItems = data["TotalItems"];
            ViewBag.ItemsCriticos = data["ItemsCriticos"];

            var items = (List<Stock>)data["Items"];
            ViewBag.ItemsBajo = items.Count(s => s.Cantidad >= s.StockMinimo && s.Cantidad <= s.StockMinimo * 2);

            return View(items);
        }

        public IActionResult TiemposReparacion()
        {
            // PATRÓN FACTORY METHOD: Crear reporte de tiempos de reparación usando factory
            var reportFactory = new TiemposReparacionReportFactory(_context);
            var report = reportFactory.CreateReport();
            var data = report.GenerateData();

            ViewBag.TotalReparaciones = data["TotalReparaciones"];
            ViewBag.TiempoPromedio = Convert.ToDouble(data["TiempoPromedioHoras"]) / 24; // Convertir a días
            ViewBag.TiempoMinimo = data["TiempoMinimoDias"];
            ViewBag.TiempoMaximo = data["TiempoMaximoDias"];

            // Datos para gráficos (serializados a JSON)
            ViewBag.RangosLabels = JsonSerializer.Serialize(data["RangosLabels"]);
            ViewBag.RangosCantidades = JsonSerializer.Serialize(data["RangosCantidades"]);
            ViewBag.Rapidos = data["Rapidos"];
            ViewBag.Normales = data["Normales"];
            ViewBag.Lentos = data["Lentos"];

            var ingresosFinalizados = (List<Ingreso>)data["Ingresos"];
            return View(ingresosFinalizados.OrderByDescending(i => i.FechaEgreso).Take(100).ToList());
        }

        // Reporte de Rentabilidad por Cliente (CRUZA: Clientes + Vehículos + Ingresos + Facturas + Detalles)
        // Contiene GRÁFICO de barras con los clientes más rentables
        public IActionResult RentabilidadClientes()
        {
            // PATRÓN FACTORY METHOD: Crear reporte de rentabilidad usando factory
            var reportFactory = new RentabilidadClientesReportFactory(_context);
            var report = reportFactory.CreateReport();
            var data = report.GenerateData();

            ViewBag.TotalClientes = data["TotalClientes"];
            ViewBag.ClientesConFactura = data["ClientesConFactura"];
            ViewBag.TotalRecaudadoGlobal = data["TotalRecaudadoGlobal"];
            ViewBag.TotalServiciosGlobal = data["TotalServiciosGlobal"];
            ViewBag.TotalRepuestosGlobal = data["TotalRepuestosGlobal"];
            ViewBag.TicketPromedioGlobal = data["TicketPromedioGlobal"];

            // Datos para los gráficos (serializados a JSON)
            ViewBag.Top10Nombres = JsonSerializer.Serialize(data["Top10Nombres"]);
            ViewBag.Top10Facturado = JsonSerializer.Serialize(data["Top10Facturado"]);
            ViewBag.Top10Servicios = JsonSerializer.Serialize(data["Top10Servicios"]);
            ViewBag.Top10Repuestos = JsonSerializer.Serialize(data["Top10Repuestos"]);

            // Datos de servicios vs repuestos globales para gráfico de torta
            ViewBag.ServiciosVsRepuestos = JsonSerializer.Serialize(new[] 
            { 
                (decimal)data["TotalServiciosGlobal"], 
                (decimal)data["TotalRepuestosGlobal"] 
            });

            ViewBag.Rentabilidad = data["Rentabilidad"];

            return View();
        }

        // Reporte de Productividad del Taller (CRUZA: Ingresos + Facturas por mes)
        // Contiene GRÁFICOS de líneas (tendencias) y torta (distribución de estados)
        public IActionResult ProductividadTaller(int? meses)
        {
            var cantMeses = meses ?? 12;
            if (cantMeses < 3) cantMeses = 3;
            if (cantMeses > 24) cantMeses = 24;

            // PATRÓN FACTORY METHOD: Crear reporte de productividad usando factory
            var reportFactory = new ProductividadTallerReportFactory(_context, cantMeses);
            var report = reportFactory.CreateReport();
            var data = report.GenerateData();

            ViewBag.TotalIngresosPeriodo = data["TotalIngresosPeriodo"];
            ViewBag.TotalFacturadoPeriodo = data["TotalFacturadoPeriodo"];
            ViewBag.PromedioIngresosMes = data["PromedioIngresosMes"];
            ViewBag.PromedioFacturacionMes = data["PromedioFacturacionMes"];
            ViewBag.MesMaxIngresos = data["MesMaxIngresos"];
            ViewBag.MesMinIngresos = data["MesMinIngresos"];
            ViewBag.TasaFinalizacion = data["TasaFinalizacion"];
            ViewBag.IngresosActivosActuales = data["IngresosActivosActuales"];
            ViewBag.Meses = data["Meses"];

            // Datos para gráficos (serializados a JSON)
            ViewBag.Labels = JsonSerializer.Serialize(data["Labels"]);
            ViewBag.IngresosCountPorMes = JsonSerializer.Serialize(data["IngresosCountPorMes"]);
            ViewBag.FacturadoPorMes = JsonSerializer.Serialize(data["FacturadoPorMes"]);
            ViewBag.FinalizadosPorMes = JsonSerializer.Serialize(data["FinalizadosPorMes"]);
            ViewBag.EstadosLabels = JsonSerializer.Serialize(data["EstadosLabels"]);
            ViewBag.EstadosCantidades = JsonSerializer.Serialize(data["EstadosCantidades"]);

            ViewBag.MesesData = data["MesesData"];

            return View();
        }

        // ═══════════════════════════════════════════════════════════
        //  EXPORTACIÓN A PDF
        // ═══════════════════════════════════════════════════════════

        public IActionResult ExportarIngresosPdf(DateTime? desde, DateTime? hasta)
        {
            desde ??= DateTime.Now.AddMonths(-1);
            hasta ??= DateTime.Now;

            var reportFactory = new IngresosPeriodoReportFactory(_context, desde.Value, hasta.Value);
            var report = reportFactory.CreateReport();
            var data = report.GenerateData();

            var pdf = _pdfService.GenerarReporteIngresosPdf(
                desde.Value, hasta.Value,
                (int)data["TotalIngresos"],
                (int)data["EnProceso"],
                (int)data["Finalizados"],
                (List<Ingreso>)data["Ingresos"]);

            return File(pdf, "application/pdf", $"Reporte_Ingresos_{desde:yyyyMMdd}_{hasta:yyyyMMdd}.pdf");
        }

        public IActionResult ExportarFacturacionPdf(DateTime? desde, DateTime? hasta)
        {
            desde ??= DateTime.Now.AddMonths(-1);
            hasta ??= DateTime.Now;

            var reportFactory = new FacturacionPeriodoReportFactory(_context, desde.Value, hasta.Value);
            var report = reportFactory.CreateReport();
            var data = report.GenerateData();

            var pdf = _pdfService.GenerarReporteFacturacionPdf(
                desde.Value, hasta.Value,
                (int)data["TotalFacturas"],
                (int)data["TotalPagadas"],
                (decimal)data["TotalRecaudado"],
                (decimal)data["TotalPendiente"],
                (List<Factura>)data["Facturas"]);

            return File(pdf, "application/pdf", $"Reporte_Facturacion_{desde:yyyyMMdd}_{hasta:yyyyMMdd}.pdf");
        }

        public IActionResult ExportarStockBajoPdf()
        {
            var reportFactory = new StockBajoReportFactory(_context);
            var report = reportFactory.CreateReport();
            var data = report.GenerateData();

            var items = (List<Stock>)data["Items"];
            var itemsBajo = items.Count(s => s.Cantidad >= s.StockMinimo && s.Cantidad <= s.StockMinimo * 2);

            var pdf = _pdfService.GenerarReporteStockBajoPdf(
                (int)data["TotalItems"],
                (int)data["ItemsCriticos"],
                itemsBajo,
                items);

            return File(pdf, "application/pdf", $"Reporte_StockBajo_{DateTime.Now:yyyyMMdd}.pdf");
        }

        public IActionResult ExportarTiemposReparacionPdf()
        {
            var reportFactory = new TiemposReparacionReportFactory(_context);
            var report = reportFactory.CreateReport();
            var data = report.GenerateData();

            var ingresos = (List<Ingreso>)data["Ingresos"];

            var pdf = _pdfService.GenerarReporteTiemposReparacionPdf(
                (int)data["TotalReparaciones"],
                Convert.ToDouble(data["TiempoPromedioHoras"]) / 24,
                Convert.ToDouble(data["TiempoMinimoDias"]),
                Convert.ToDouble(data["TiempoMaximoDias"]),
                ingresos.OrderByDescending(i => i.FechaEgreso).Take(100).ToList());

            return File(pdf, "application/pdf", $"Reporte_TiemposReparacion_{DateTime.Now:yyyyMMdd}.pdf");
        }

        public IActionResult ExportarRentabilidadPdf()
        {
            var reportFactory = new RentabilidadClientesReportFactory(_context);
            var report = reportFactory.CreateReport();
            var data = report.GenerateData();

            var pdf = _pdfService.GenerarReporteRentabilidadPdf(
                (int)data["TotalClientes"],
                (int)data["ClientesConFactura"],
                (decimal)data["TotalRecaudadoGlobal"],
                (decimal)data["TicketPromedioGlobal"],
                data["Rentabilidad"]);

            return File(pdf, "application/pdf", $"Reporte_Rentabilidad_{DateTime.Now:yyyyMMdd}.pdf");
        }

        public IActionResult ExportarProductividadPdf(int? meses)
        {
            var cantMeses = meses ?? 12;
            if (cantMeses < 3) cantMeses = 3;
            if (cantMeses > 24) cantMeses = 24;

            var reportFactory = new ProductividadTallerReportFactory(_context, cantMeses);
            var report = reportFactory.CreateReport();
            var data = report.GenerateData();

            var pdf = _pdfService.GenerarReporteProductividadPdf(
                (int)data["TotalIngresosPeriodo"],
                (decimal)data["TotalFacturadoPeriodo"],
                (double)data["PromedioIngresosMes"],
                (decimal)data["PromedioFacturacionMes"],
                (double)data["TasaFinalizacion"],
                data["MesesData"]);

            return File(pdf, "application/pdf", $"Reporte_Productividad_{DateTime.Now:yyyyMMdd}.pdf");
        }
    }
}