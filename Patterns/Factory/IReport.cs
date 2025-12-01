namespace AutoSys.Patterns.Factory
{
    /// <summary>
    /// Patrón Factory Method: Producto base para reportes
    /// </summary>
    public interface IReport
    {
        string GetTitle();
        string GetDescription();
        Dictionary<string, object> GenerateData();
        string GetReportType();
    }
}
