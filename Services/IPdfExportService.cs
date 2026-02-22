namespace AutoSys.Services
{
    /// <summary>
    /// Servicio de exportación de reportes a PDF.
    /// Permite generar archivos PDF a partir de los datos del Factory Method de reportes.
    /// </summary>
    public interface IPdfExportService
    {
        byte[] GenerarReporteIngresosPdf(DateTime desde, DateTime hasta, int totalIngresos, int enProceso, int finalizados, IEnumerable<Models.Ingreso> ingresos);
        byte[] GenerarReporteFacturacionPdf(DateTime desde, DateTime hasta, int totalFacturas, int totalPagadas, decimal totalRecaudado, decimal totalPendiente, IEnumerable<Models.Factura> facturas);
        byte[] GenerarReporteStockBajoPdf(int totalItems, int itemsCriticos, int itemsBajo, IEnumerable<Models.Stock> items);
        byte[] GenerarReporteTiemposReparacionPdf(int totalReparaciones, double tiempoPromedio, double tiempoMinimo, double tiempoMaximo, IEnumerable<Models.Ingreso> ingresos);
        byte[] GenerarReporteRentabilidadPdf(int totalClientes, int clientesConFactura, decimal totalRecaudado, decimal ticketPromedio, dynamic rentabilidad);
        byte[] GenerarReporteProductividadPdf(int totalIngresos, decimal totalFacturado, double promedioIngresosMes, decimal promedioFacturacionMes, double tasaFinalizacion, dynamic mesesData);
    }
}
