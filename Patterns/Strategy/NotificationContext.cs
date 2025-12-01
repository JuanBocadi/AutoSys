namespace AutoSys.Patterns.Strategy
{
    /// <summary>
    /// Patrón Strategy: Contexto que usa las estrategias de notificación
    /// Permite cambiar dinámicamente el tipo de notificación
    /// </summary>
    public class NotificationContext
    {
        private INotificationStrategy _strategy;

        public NotificationContext(INotificationStrategy strategy)
        {
            _strategy = strategy;
        }

        public void SetStrategy(INotificationStrategy strategy)
        {
            _strategy = strategy;
        }

        public async Task SendNotificationAsync(string destination, string subject, string message)
        {
            await _strategy.SendAsync(destination, subject, message);
        }

        public string GetCurrentNotificationType()
        {
            return _strategy.GetNotificationType();
        }
    }
}
