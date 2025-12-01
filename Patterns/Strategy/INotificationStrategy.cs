namespace AutoSys.Patterns.Strategy
{
    /// <summary>
    /// Patrón Strategy: Define una familia de algoritmos de notificación
    /// Permite cambiar el algoritmo de notificación en tiempo de ejecución
    /// </summary>
    public interface INotificationStrategy
    {
        Task SendAsync(string destination, string subject, string message);
        string GetNotificationType();
    }
}
