using Microsoft.Extensions.Logging;
using AutoSys.Patterns.Strategy;

namespace AutoSys.Services
{
    /// <summary>
    /// Servicio de notificaciones que usa el patrón Strategy
    /// Permite cambiar el tipo de notificación dinámicamente
    /// </summary>
    public class NotificationService : INotificationService
    {
        private readonly ILogger<NotificationService> _logger;
        private readonly NotificationContext _notificationContext;

        public NotificationService(ILogger<NotificationService> logger)
        {
            _logger = logger;
            
            // Por defecto usa LogNotificationStrategy
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var strategyLogger = loggerFactory.CreateLogger<LogNotificationStrategy>();
            var defaultStrategy = new LogNotificationStrategy(strategyLogger);
            _notificationContext = new NotificationContext(defaultStrategy);
        }

        public async Task SendAsync(string destination, string subject, string message)
        {
            _logger.LogInformation("Enviando notificación a {Destination}: {Subject}", destination, subject);
            
            try
            {
                await _notificationContext.SendNotificationAsync(destination, subject, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar notificación a {Destination}", destination);
            }
        }

        // Método para cambiar la estrategia de notificación
        public void SetNotificationStrategy(INotificationStrategy strategy)
        {
            _notificationContext.SetStrategy(strategy);
            _logger.LogInformation("Estrategia de notificación cambiada a: {Type}", 
                strategy.GetNotificationType());
        }
    }
}


