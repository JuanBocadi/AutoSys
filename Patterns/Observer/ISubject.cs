namespace AutoSys.Patterns.Observer
{
    /// <summary>
    /// Patrón Observer: Sujeto que notifica a los observadores
    /// </summary>
    public interface ISubject
    {
        void Attach(IObserver observer);
        void Detach(IObserver observer);
        Task NotifyAsync(string eventType, object data);
    }
}
