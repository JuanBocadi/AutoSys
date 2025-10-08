namespace AutoSys.Services
{
    public interface INotificationService
    {
        Task SendAsync(string destination, string subject, string message);
    }
}


