using Microsoft.Extensions.Logging;

namespace AutoSys.Patterns.Strategy
{
    /// <summary>
    /// Patrón Strategy: Estrategias concretas de notificación
    /// </summary>
    public class EmailNotificationStrategy : INotificationStrategy
    {
        private readonly ILogger<EmailNotificationStrategy> _logger;

        public EmailNotificationStrategy(ILogger<EmailNotificationStrategy> logger)
        {
            _logger = logger;
        }

        public Task SendAsync(string destination, string subject, string message)
        {
            // TODO: Implementar envío real de email usando SMTP o servicio de terceros
            _logger.LogInformation("📧 Email enviado a {Destination}: {Subject}", destination, subject);
            _logger.LogDebug("Contenido del email: {Message}", message);
            return Task.CompletedTask;
        }

        public string GetNotificationType() => "Email";
    }

    public class SmsNotificationStrategy : INotificationStrategy
    {
        private readonly ILogger<SmsNotificationStrategy> _logger;

        public SmsNotificationStrategy(ILogger<SmsNotificationStrategy> logger)
        {
            _logger = logger;
        }

        public Task SendAsync(string destination, string subject, string message)
        {
            // TODO: Implementar envío real de SMS usando Twilio u otro servicio
            _logger.LogInformation("📱 SMS enviado a {Destination}: {Subject}", destination, subject);
            _logger.LogDebug("Contenido del SMS: {Message}", message);
            return Task.CompletedTask;
        }

        public string GetNotificationType() => "SMS";
    }

    public class WhatsAppNotificationStrategy : INotificationStrategy
    {
        private readonly ILogger<WhatsAppNotificationStrategy> _logger;

        public WhatsAppNotificationStrategy(ILogger<WhatsAppNotificationStrategy> logger)
        {
            _logger = logger;
        }

        public Task SendAsync(string destination, string subject, string message)
        {
            // TODO: Implementar envío real de WhatsApp usando API oficial
            _logger.LogInformation("💬 WhatsApp enviado a {Destination}: {Subject}", destination, subject);
            _logger.LogDebug("Contenido del WhatsApp: {Message}", message);
            return Task.CompletedTask;
        }

        public string GetNotificationType() => "WhatsApp";
    }

    public class LogNotificationStrategy : INotificationStrategy
    {
        private readonly ILogger<LogNotificationStrategy> _logger;

        public LogNotificationStrategy(ILogger<LogNotificationStrategy> logger)
        {
            _logger = logger;
        }

        public Task SendAsync(string destination, string subject, string message)
        {
            _logger.LogInformation("📋 Notificación registrada para {Destination}: {Subject} - {Message}", 
                destination, subject, message);
            return Task.CompletedTask;
        }

        public string GetNotificationType() => "Log";
    }
}
