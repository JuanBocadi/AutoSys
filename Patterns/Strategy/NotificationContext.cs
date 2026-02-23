namespace AutoSys.Patterns.Strategy
{
    /// Contexto que mantiene la estrategia de notificación activa y delega el envío
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
