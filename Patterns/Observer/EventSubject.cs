namespace AutoSys.Patterns.Observer
{
    /// <summary>
    /// Patrón Observer: Sujeto concreto que gestiona observadores
    /// </summary>
    public class EventSubject : ISubject
    {
        private readonly List<IObserver> _observers = new();
        private readonly ILogger<EventSubject> _logger;

        public EventSubject(ILogger<EventSubject> logger)
        {
            _logger = logger;
        }

        public void Attach(IObserver observer)
        {
            if (!_observers.Contains(observer))
            {
                _observers.Add(observer);
                _logger.LogInformation("Observer adjuntado: {ObserverName}", observer.GetObserverName());
            }
        }

        public void Detach(IObserver observer)
        {
            if (_observers.Contains(observer))
            {
                _observers.Remove(observer);
                _logger.LogInformation("Observer removido: {ObserverName}", observer.GetObserverName());
            }
        }

        public async Task NotifyAsync(string eventType, object data)
        {
            _logger.LogInformation("Notificando a {Count} observadores del evento: {EventType}", 
                _observers.Count, eventType);

            foreach (var observer in _observers)
            {
                try
                {
                    await observer.UpdateAsync(eventType, data);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al notificar al observer: {ObserverName}", 
                        observer.GetObserverName());
                }
            }
        }

        public int GetObserverCount() => _observers.Count;
    }
}
