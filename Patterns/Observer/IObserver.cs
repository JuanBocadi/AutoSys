namespace AutoSys.Patterns.Observer
{
    /// <summary>
    /// Patrón Observer: Observador que reacciona a eventos
    /// </summary>
    public interface IObserver
    {
        Task UpdateAsync(string eventType, object data);
        string GetObserverName();
    }
}
