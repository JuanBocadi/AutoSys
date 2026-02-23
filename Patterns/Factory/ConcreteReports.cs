using AutoSys.Data;
using AutoSys.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoSys.Patterns.Factory
{
    /// Reportes concretos que implementan IReport con sus consultas específicas a la BD
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

            var promedioDias = tiempos.Any() ? tiempos.Average(t => t.Dias) : 0;

            // Distribución por rangos de tiempo (para gráfico de barras/histograma)
            var rangosLabels = new List<string> { "0-1 día", "1-3 días", "3-5 días", "5-7 días", "7-10 días", "10+ días" };
            var rangosCantidades = new List<int>
            {
                tiempos.Count(t => t.Dias <= 1),
                tiempos.Count(t => t.Dias > 1 && t.Dias <= 3),
                tiempos.Count(t => t.Dias > 3 && t.Dias <= 5),
                tiempos.Count(t => t.Dias > 5 && t.Dias <= 7),
                tiempos.Count(t => t.Dias > 7 && t.Dias <= 10),
                tiempos.Count(t => t.Dias > 10)
            };

            // Distribución de rendimiento (para gráfico de torta)
            var rapidos = tiempos.Count(t => t.Dias <= promedioDias);
            var normales = tiempos.Count(t => t.Dias > promedioDias && t.Dias <= promedioDias * 1.5);
            var lentos = tiempos.Count(t => t.Dias > promedioDias * 1.5);

            var data = new Dictionary<string, object>
            {
                { "TotalReparaciones", ingresos.Count },
                { "TiempoPromedioHoras", tiempos.Any() ? tiempos.Average(t => t.Dias * 24) : 0 },
                { "TiempoMinimoDias", tiempos.Any() ? tiempos.Min(t => t.Dias) : 0 },
                { "TiempoMaximoDias", tiempos.Any() ? tiempos.Max(t => t.Dias) : 0 },
                { "Ingresos", ingresos },
                // Datos para gráficos
                { "RangosLabels", rangosLabels },
                { "RangosCantidades", rangosCantidades },
                { "Rapidos", rapidos },
                { "Normales", normales },
                { "Lentos", lentos }
            };

            return data;
        }
    }

    /// Reporte de rentabilidad por cliente - cruza datos de facturación e ingresos por cliente
    public class RentabilidadClientesReport : IReport
    {
        private readonly AutoSysDbContext _context;

        public RentabilidadClientesReport(AutoSysDbContext context)
        {
            _context = context;
        }

        public string GetTitle() => "Reporte de Rentabilidad por Cliente";

        public string GetDescription() => "Análisis de ingresos generados por cliente, cruzando datos de facturación, vehículos y reparaciones";

        public string GetReportType() => "RentabilidadClientes";

        public Dictionary<string, object> GenerateData()
        {
            // Cruzar: Clientes → Vehículos → Ingresos + Clientes → Facturas → Detalles
            var clientes = _context.Clientes
                .Include(c => c.Vehiculos)
                    .ThenInclude(v => v.Ingresos)
                .ToList();

            var facturas = _context.Facturas
                .Include(f => f.Detalles)
                .Include(f => f.Cliente)
                .Where(f => f.Estado != "Anulada")
                .ToList();

            // Procesar datos: calcular métricas de rentabilidad por cliente
            var rentabilidad = clientes.Select(c =>
            {
                var facturasCliente = facturas.Where(f => f.ClienteId == c.Id).ToList();
                var totalIngresos = c.Vehiculos.Sum(v => v.Ingresos.Count);
                var totalFacturado = facturasCliente.Sum(f => f.Total);
                var totalServicios = facturasCliente.SelectMany(f => f.Detalles)
                    .Where(d => d.Tipo == "Servicio").Sum(d => d.Subtotal);
                var totalRepuestos = facturasCliente.SelectMany(f => f.Detalles)
                    .Where(d => d.Tipo == "Repuesto").Sum(d => d.Subtotal);
                var ticketPromedio = facturasCliente.Any() ? facturasCliente.Average(f => f.Total) : 0m;
                var ultimaVisita = c.Vehiculos.SelectMany(v => v.Ingresos)
                    .OrderByDescending(i => i.FechaIngreso)
                    .FirstOrDefault()?.FechaIngreso;
                var primeraVisita = c.Vehiculos.SelectMany(v => v.Ingresos)
                    .OrderBy(i => i.FechaIngreso)
                    .FirstOrDefault()?.FechaIngreso;

                // Frecuencia: ingresos por mes desde la primera visita
                double frecuenciaMensual = 0;
                if (primeraVisita.HasValue && totalIngresos > 0)
                {
                    var meses = Math.Max(1, (DateTime.Now - primeraVisita.Value).TotalDays / 30.0);
                    frecuenciaMensual = totalIngresos / meses;
                }

                return new
                {
                    Cliente = c,
                    TotalVehiculos = c.Vehiculos.Count,
                    TotalIngresos = totalIngresos,
                    TotalFacturado = totalFacturado,
                    TotalServicios = totalServicios,
                    TotalRepuestos = totalRepuestos,
                    TicketPromedio = ticketPromedio,
                    CantidadFacturas = facturasCliente.Count,
                    UltimaVisita = ultimaVisita,
                    PrimeraVisita = primeraVisita,
                    FrecuenciaMensual = frecuenciaMensual
                };
            })
            .OrderByDescending(r => r.TotalFacturado)
            .ToList();

            // Métricas globales procesadas
            var totalRecaudadoGlobal = rentabilidad.Sum(r => r.TotalFacturado);
            var totalServiciosGlobal = rentabilidad.Sum(r => r.TotalServicios);
            var totalRepuestosGlobal = rentabilidad.Sum(r => r.TotalRepuestos);
            var ticketPromedioGlobal = facturas.Any() ? facturas.Average(f => f.Total) : 0m;
            var clientesConFactura = rentabilidad.Count(r => r.CantidadFacturas > 0);

            // Top 10 para el gráfico
            var top10 = rentabilidad.Take(10).ToList();

            var data = new Dictionary<string, object>
            {
                { "Rentabilidad", rentabilidad },
                { "Top10", top10 },
                { "TotalClientes", clientes.Count },
                { "ClientesConFactura", clientesConFactura },
                { "TotalRecaudadoGlobal", totalRecaudadoGlobal },
                { "TotalServiciosGlobal", totalServiciosGlobal },
                { "TotalRepuestosGlobal", totalRepuestosGlobal },
                { "TicketPromedioGlobal", ticketPromedioGlobal },
                // Datos para gráficos (serializados como listas simples)
                { "Top10Nombres", top10.Select(r => $"{r.Cliente.Nombre} {r.Cliente.Apellido}").ToList() },
                { "Top10Facturado", top10.Select(r => r.TotalFacturado).ToList() },
                { "Top10Servicios", top10.Select(r => r.TotalServicios).ToList() },
                { "Top10Repuestos", top10.Select(r => r.TotalRepuestos).ToList() }
            };

            return data;
        }
    }

    /// Reporte de productividad del taller - tendencias mensuales de ingresos y facturación
    public class ProductividadTallerReport : IReport
    {
        private readonly AutoSysDbContext _context;
        private readonly int _meses;

        public ProductividadTallerReport(AutoSysDbContext context, int meses = 12)
        {
            _context = context;
            _meses = meses;
        }

        public string GetTitle() => "Reporte de Productividad del Taller";

        public string GetDescription() => $"Análisis de tendencias de trabajo y facturación de los últimos {_meses} meses";

        public string GetReportType() => "ProductividadTaller";

        public Dictionary<string, object> GenerateData()
        {
            var fechaDesde = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(-(_meses - 1));

            // Cruzar: Ingresos (volumen de trabajo) + Facturas (facturación) por mes
            var ingresos = _context.Ingresos
                .Include(i => i.Vehiculo!)
                    .ThenInclude(v => v.Cliente)
                .Where(i => i.FechaIngreso >= fechaDesde)
                .ToList();

            var facturas = _context.Facturas
                .Include(f => f.Detalles)
                .Where(f => f.FechaEmision >= fechaDesde && f.Estado != "Anulada")
                .ToList();

            // Ingresos activos (sin fecha de egreso)
            var ingresosActivos = _context.Ingresos
                .Where(i => !i.FechaEgreso.HasValue)
                .ToList();

            // Generar datos mensuales
            var mesesData = new List<object>();
            var labels = new List<string>();
            var ingresosCountPorMes = new List<int>();
            var facturadoPorMes = new List<decimal>();
            var finalizadosPorMes = new List<int>();

            for (int i = 0; i < _meses; i++)
            {
                var mesDate = fechaDesde.AddMonths(i);
                var mesInicio = new DateTime(mesDate.Year, mesDate.Month, 1);
                var mesFin = mesInicio.AddMonths(1);

                var ingMes = ingresos.Where(ing => ing.FechaIngreso >= mesInicio && ing.FechaIngreso < mesFin).ToList();
                var facMes = facturas.Where(f => f.FechaEmision >= mesInicio && f.FechaEmision < mesFin).ToList();
                var finalizados = ingresos.Where(ing => ing.FechaEgreso.HasValue && ing.FechaEgreso.Value >= mesInicio && ing.FechaEgreso.Value < mesFin).Count();

                labels.Add(mesDate.ToString("MMM yyyy"));
                ingresosCountPorMes.Add(ingMes.Count);
                facturadoPorMes.Add(facMes.Sum(f => f.Total));
                finalizadosPorMes.Add(finalizados);

                mesesData.Add(new
                {
                    Mes = mesDate.ToString("MMMM yyyy"),
                    Ingresos = ingMes.Count,
                    Facturado = facMes.Sum(f => f.Total),
                    CantFacturas = facMes.Count,
                    Finalizados = finalizados,
                    TicketPromedio = facMes.Any() ? facMes.Average(f => f.Total) : 0m
                });
            }

            // Distribución de estados de ingresos activos (para gráfico de torta)
            var estadosDistribucion = ingresosActivos
                .GroupBy(i => i.Estado)
                .Select(g => new { Estado = g.Key, Cantidad = g.Count() })
                .OrderByDescending(e => e.Cantidad)
                .ToList();

            // Métricas de productividad procesadas
            var totalIngresosperiodo = ingresos.Count;
            var totalFacturadoPeriodo = facturas.Sum(f => f.Total);
            var promedioIngresosMes = _meses > 0 ? (double)totalIngresosperiodo / _meses : 0;
            var promedioFacturacionMes = _meses > 0 ? totalFacturadoPeriodo / _meses : 0;
            var mesMaxIngresos = ingresosCountPorMes.Any() ? ingresosCountPorMes.Max() : 0;
            var mesMinIngresos = ingresosCountPorMes.Any() ? ingresosCountPorMes.Min() : 0;
            var tasaFinalizacion = totalIngresosperiodo > 0 
                ? Math.Round((double)ingresos.Count(i => i.FechaEgreso.HasValue) / totalIngresosperiodo * 100, 1)
                : 0;

            var data = new Dictionary<string, object>
            {
                { "MesesData", mesesData },
                { "Labels", labels },
                { "IngresosCountPorMes", ingresosCountPorMes },
                { "FacturadoPorMes", facturadoPorMes },
                { "FinalizadosPorMes", finalizadosPorMes },
                { "EstadosDistribucion", estadosDistribucion },
                { "EstadosLabels", estadosDistribucion.Select(e => e.Estado).ToList() },
                { "EstadosCantidades", estadosDistribucion.Select(e => e.Cantidad).ToList() },
                { "TotalIngresosPeriodo", totalIngresosperiodo },
                { "TotalFacturadoPeriodo", totalFacturadoPeriodo },
                { "PromedioIngresosMes", promedioIngresosMes },
                { "PromedioFacturacionMes", promedioFacturacionMes },
                { "MesMaxIngresos", mesMaxIngresos },
                { "MesMinIngresos", mesMinIngresos },
                { "TasaFinalizacion", tasaFinalizacion },
                { "IngresosActivosActuales", ingresosActivos.Count },
                { "Meses", _meses }
            };

            return data;
        }
    }
}
