namespace AutoSys.Patterns.Factory
{
    /// Interfaz base para todos los reportes del sistema
    public interface IReport
    {
        string GetTitle();
        string GetDescription();
        Dictionary<string, object> GenerateData();
        string GetReportType();
    }
}
