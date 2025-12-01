using AutoSys.Data;

namespace AutoSys.Patterns.Factory
{
    /// <summary>
    /// Patrón Factory Method: Creador abstracto
    /// </summary>
    public abstract class ReportFactory
    {
        protected readonly AutoSysDbContext _context;

        protected ReportFactory(AutoSysDbContext context)
        {
            _context = context;
        }

        // Factory Method
        public abstract IReport CreateReport();

        // Método template que usa el factory method
        public Dictionary<string, object> GenerateReport()
        {
            var report = CreateReport();
            return report.GenerateData();
        }
    }

    /// <summary>
    /// Patrón Factory Method: Creadores concretos
    /// </summary>
    public class ClientesActivosReportFactory : ReportFactory
    {
        public ClientesActivosReportFactory(AutoSysDbContext context) : base(context) { }

        public override IReport CreateReport()
        {
            return new ClientesActivosReport(_context);
        }
    }

    public class FacturacionPeriodoReportFactory : ReportFactory
    {
        private readonly DateTime _desde;
        private readonly DateTime _hasta;

        public FacturacionPeriodoReportFactory(AutoSysDbContext context, DateTime desde, DateTime hasta) 
            : base(context)
        {
            _desde = desde;
            _hasta = hasta;
        }

        public override IReport CreateReport()
        {
            return new FacturacionPeriodoReport(_context, _desde, _hasta);
        }
    }

    public class IngresosPeriodoReportFactory : ReportFactory
    {
        private readonly DateTime _desde;
        private readonly DateTime _hasta;

        public IngresosPeriodoReportFactory(AutoSysDbContext context, DateTime desde, DateTime hasta) 
            : base(context)
        {
            _desde = desde;
            _hasta = hasta;
        }

        public override IReport CreateReport()
        {
            return new IngresosPeriodoReport(_context, _desde, _hasta);
        }
    }

    public class StockBajoReportFactory : ReportFactory
    {
        public StockBajoReportFactory(AutoSysDbContext context) : base(context) { }

        public override IReport CreateReport()
        {
            return new StockBajoReport(_context);
        }
    }

    public class TiemposReparacionReportFactory : ReportFactory
    {
        public TiemposReparacionReportFactory(AutoSysDbContext context) : base(context) { }

        public override IReport CreateReport()
        {
            return new TiemposReparacionReport(_context);
        }
    }
}
