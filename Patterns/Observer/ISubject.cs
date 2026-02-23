namespace AutoSys.Patterns.Observer
{
    /// Contrato del sujeto que emite eventos y notifica a los observadores
    public interface ISubject
    {
        void Attach(IObserver observer);
        void Detach(IObserver observer);
        Task NotifyAsync(string eventType, object data);
    }
}
