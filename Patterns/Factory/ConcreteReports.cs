using AutoSys.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoSys.Patterns.Factory
{
    /// <summary>
    /// Patrón Factory Method: Productos concretos - Diferentes tipos de reportes
    /// </summary>
    public class ClientesActivosReport : IReport
    {
        private readonly AutoSysDbContext _context;

        public ClientesActivosReport(AutoSysDbContext context)
        {
            _context = context;
        }

        public string GetTitle() => "Reporte de Clientes Activos";

        public string GetDescription() => "Listado de clientes con actividad en el sistema";

        public string GetReportType() => "ClientesActivos";

        public Dictionary<string, object> GenerateData()
        {
            var clientes = _context.Clientes
                .Include(c => c.Vehiculos)
                .ThenInclude(v => v.Ingresos)
                .ToList();

            var data = new Dictionary<string, object>
            {
                { "TotalClientes", clientes.Count },
                { "ClientesConVehiculos", clientes.Count(c => c.Vehiculos.Any()) },
                { "ClientesConIngresos", clientes.Count(c => c.Vehiculos.Any(v => v.Ingresos.Any())) },
                { "Clientes", clientes }
            };

            return data;
        }
    }

    public class FacturacionPeriodoReport : IReport
    {
        private readonly AutoSysDbContext _context;
        private readonly DateTime _desde;
        private readonly DateTime _hasta;

        public FacturacionPeriodoReport(AutoSysDbContext context, DateTime desde, DateTime hasta)
        {
            _context = context;
            _desde = desde;
            _hasta = hasta;
        }

        public string GetTitle() => "Reporte de Facturación por Período";

        public string GetDescription() => $"Facturación desde {_desde:dd/MM/yyyy} hasta {_hasta:dd/MM/yyyy}";

        public string GetReportType() => "FacturacionPorPeriodo";

        public Dictionary<string, object> GenerateData()
        {
            var facturas = _context.Facturas
                .Include(f => f.Cliente)
                .Include(f => f.Detalles)
                .Where(f => f.FechaEmision >= _desde && f.FechaEmision <= _hasta)
                .ToList();

            var data = new Dictionary<string, object>
            {
                { "TotalFacturas", facturas.Count },
                { "TotalPagadas", facturas.Count(f => f.Estado == "Pagada") },
                { "TotalRecaudado", facturas.Where(f => f.Estado == "Pagada").Sum(f => f.Total) },
                { "TotalPendiente", facturas.Where(f => f.Estado == "Pendiente").Sum(f => f.Total) },
                { "Facturas", facturas },
                { "Desde", _desde },
                { "Hasta", _hasta }
            };

            return data;
        }
    }

    public class IngresosPeriodoReport : IReport
    {
        private readonly AutoSysDbContext _context;
        private readonly DateTime _desde;
        private readonly DateTime _hasta;

        public IngresosPeriodoReport(AutoSysDbContext context, DateTime desde, DateTime hasta)
        {
            _context = context;
            _desde = desde;
            _hasta = hasta;
        }

        public string GetTitle() => "Reporte de Ingresos por Período";

        public string GetDescription() => $"Ingresos desde {_desde:dd/MM/yyyy} hasta {_hasta:dd/MM/yyyy}";

        public string GetReportType() => "IngresosPorPeriodo";

        public Dictionary<string, object> GenerateData()
        {
            var ingresos = _context.Ingresos
                .Include(i => i.Vehiculo!)
                .ThenInclude(v => v.Cliente)
                .Where(i => i.FechaIngreso >= _desde && i.FechaIngreso <= _hasta)
                .ToList();

            var data = new Dictionary<string, object>
            {
                { "TotalIngresos", ingresos.Count },
                { "EnProceso", ingresos.Count(i => !i.FechaEgreso.HasValue) },
                { "Finalizados", ingresos.Count(i => i.FechaEgreso.HasValue) },
                { "Ingresos", ingresos },
                { "Desde", _desde },
                { "Hasta", _hasta }
            };

            return data;
        }
    }

    public class StockBajoReport : IReport
    {
        private readonly AutoSysDbContext _context;

        public StockBajoReport(AutoSysDbContext context)
        {
            _context = context;
        }

        public string GetTitle() => "Reporte de Stock Bajo";

        public string GetDescription() => "Productos con stock por debajo del mínimo";

        public string GetReportType() => "StockBajo";

        public Dictionary<string, object> GenerateData()
        {
            var items = _context.Stock
                .Where(s => s.Cantidad < s.StockMinimo)
                .OrderBy(s => s.Cantidad)
                .ToList();

            var data = new Dictionary<string, object>
            {
                { "TotalItems", items.Count },
                { "ItemsCriticos", items.Count(s => s.Cantidad == 0) },
                { "Items", items }
            };

            return data;
        }
    }

    public class TiemposReparacionReport : IReport
    {
        private readonly AutoSysDbContext _context;

        public TiemposReparacionReport(AutoSysDbContext context)
        {
            _context = context;
        }

        public string GetTitle() => "Reporte de Tiempos de Reparación";

        public string GetDescription() => "Análisis de tiempos promedio de reparación";

        public string GetReportType() => "TiemposReparacion";

        public Dictionary<string, object> GenerateData()
        {
            var ingresos = _context.Ingresos
                .Include(i => i.Vehiculo!)
                .ThenInclude(v => v.Cliente)
                .Where(i => i.FechaEgreso.HasValue)
                .ToList();

            var tiempos = ingresos.Select(i => new
            {
                Ingreso = i,
                Dias = (i.FechaEgreso!.Value - i.FechaIngreso).TotalDays
            }).ToList();

            var data = new Dictionary<string, object>
            {
                { "TotalReparaciones", ingresos.Count },
                { "TiempoPromedioHoras", tiempos.Any() ? tiempos.Average(t => t.Dias * 24) : 0 },
                { "TiempoMinimoDias", tiempos.Any() ? tiempos.Min(t => t.Dias) : 0 },
                { "TiempoMaximoDias", tiempos.Any() ? tiempos.Max(t => t.Dias) : 0 },
                { "Ingresos", ingresos }
            };

            return data;
        }
    }
}
