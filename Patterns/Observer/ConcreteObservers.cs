using AutoSys.Services;

namespace AutoSys.Patterns.Observer
{
    /// Observadores concretos: EmailNotificationObserver envía emails, LoggerObserver registra en consola
    public class EmailNotificationObserver : IObserver
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<EmailNotificationObserver> _logger;

        public EmailNotificationObserver(INotificationService notificationService, 
                                         ILogger<EmailNotificationObserver> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task UpdateAsync(string eventType, object data)
        {
            _logger.LogInformation("EmailObserver notificado del evento: {EventType}", eventType);

            if (data is IngresoStateChangedEventData ingresoData)
            {
                string destination = ingresoData.ClienteEmail ?? "cliente@example.com";

                if (ingresoData.NuevoEstado == "Finalizado")
                {
                    // Email 1: Vehículo listo para retirar
                    string subject = $"🔧 Su vehículo está listo para retirar - {ingresoData.Patente}";
                    string message = $"<h2>¡Su vehículo está listo!</h2>" +
                                    $"<p>Le informamos que la reparación de su vehículo <strong>{ingresoData.Patente}</strong> " +
                                    $"ha sido <strong>finalizada</strong>.</p>" +
                                    $"<p>Puede pasar a retirarlo por nuestro taller en el horario de atención.</p>" +
                                    $"<p>Ingreso #{ingresoData.IngresoId}</p>" +
                                    $"<br><p>Saludos,<br><strong>AutoSys - Taller Mecánico</strong></p>";

                    await _notificationService.SendAsync(destination, subject, message);
                    _logger.LogInformation("📧 Email de 'Finalizado' enviado a {Destination}", destination);
                }
                else if (ingresoData.NuevoEstado == "Entregado")
                {
                    // Email 2: Confirmación de entrega
                    string subject = $"✅ Vehículo entregado - {ingresoData.Patente}";
                    string message = $"<h2>Confirmación de entrega</h2>" +
                                    $"<p>Le confirmamos que su vehículo <strong>{ingresoData.Patente}</strong> " +
                                    $"ha sido <strong>entregado</strong> exitosamente.</p>" +
                                    $"<p>Gracias por confiar en nuestro taller. ¡Esperamos verlo pronto!</p>" +
                                    $"<p>Ingreso #{ingresoData.IngresoId}</p>" +
                                    $"<br><p>Saludos,<br><strong>AutoSys - Taller Mecánico</strong></p>";

                    await _notificationService.SendAsync(destination, subject, message);
                    _logger.LogInformation("📧 Email de 'Entregado' enviado a {Destination}", destination);
                }
                else
                {
                    _logger.LogInformation("Estado cambiado a '{Estado}' - no se envía email", 
                        ingresoData.NuevoEstado);
                }
            }
        }

        public string GetObserverName() => "Email Notification Observer";
    }

    public class LoggerObserver : IObserver
    {
        private readonly ILogger<LoggerObserver> _logger;

        public LoggerObserver(ILogger<LoggerObserver> logger)
        {
            _logger = logger;
        }

        public Task UpdateAsync(string eventType, object data)
        {
            _logger.LogInformation("📝 Evento registrado: {EventType}", eventType);
            
            if (data is IngresoStateChangedEventData ingresoData)
            {
                _logger.LogInformation("Ingreso #{Id} cambió de estado a: {Estado} (Vehículo: {Patente})",
                    ingresoData.IngresoId, ingresoData.NuevoEstado, ingresoData.Patente);
            }


            return Task.CompletedTask;
        }

        public string GetObserverName() => "Logger Observer";
    }

    // Clases de datos para eventos
    public class IngresoStateChangedEventData
    {
        public int IngresoId { get; set; }
        public string? EstadoAnterior { get; set; }
        public string NuevoEstado { get; set; } = string.Empty;
        public string Patente { get; set; } = string.Empty;
        public string? ClienteEmail { get; set; }
        public DateTime FechaCambio { get; set; }
    }


}