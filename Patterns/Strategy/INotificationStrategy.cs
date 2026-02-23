namespace AutoSys.Patterns.Strategy
{
    /// Contrato para las estrategias de envío de notificaciones
    public interface INotificationStrategy
    {
        Task SendAsync(string destination, string subject, string message);
        string GetNotificationType();
    }
}
