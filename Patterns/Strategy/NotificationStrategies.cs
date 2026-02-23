using Microsoft.Extensions.Logging;
using AutoSys.Services;

namespace AutoSys.Patterns.Strategy
{
    /// Patrón Strategy: Estrategias concretas de notificación
    public class EmailNotificationStrategy : INotificationStrategy
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<EmailNotificationStrategy> _logger;

        public EmailNotificationStrategy(IEmailService emailService, ILogger<EmailNotificationStrategy> logger)
        {
            _emailService = emailService;
            _logger = logger;
        }

        public async Task SendAsync(string destination, string subject, string message)
        {
            try
            {
                // Envío real de email usando el mismo servicio SMTP que recuperar contraseña
                await _emailService.SendEmailAsync(destination, subject, message);
                _logger.LogInformation("📧 Email enviado exitosamente a {Destination}: {Subject}", destination, subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "📧 Error al enviar email a {Destination}: {Subject}", destination, subject);
                throw;
            }
        }

        public string GetNotificationType() => "Email";
    }
}
