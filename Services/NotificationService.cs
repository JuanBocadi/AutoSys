using Microsoft.Extensions.Logging;

namespace AutoSys.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(ILogger<NotificationService> logger)
        {
            _logger = logger;
        }

        public Task SendAsync(string destination, string subject, string message)
        {
            // TODO: Reemplazar por Email/WhatsApp provider
            _logger.LogInformation("Notificación a {Destination}: {Subject} - {Message}", destination, subject, message);
            return Task.CompletedTask;
        }
    }
}


