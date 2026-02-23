namespace AutoSys.Patterns.Observer
{
    /// Contrato para los observadores que reaccionan a eventos del sistema
    public interface IObserver
    {
        Task UpdateAsync(string eventType, object data);
        string GetObserverName();
    }
}
