using AutoSys.Services;

namespace AutoSys.Patterns.Observer
{
    /// <summary>
    /// Patrón Observer: Observadores concretos que reaccionan a eventos
    /// </summary>
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
                string subject = $"Actualización de su vehículo - {ingresoData.Patente}";
                string message = $"El estado de su vehículo {ingresoData.Patente} ha cambiado a: {ingresoData.NuevoEstado}. " +
                                $"Ingreso #{ingresoData.IngresoId}.";
                
                await _notificationService.SendAsync(destination, subject, message);
            }
            else if (data is StockBajoEventData stockData)
            {
                string destination = "admin@autosys.com";
                string subject = $"Alerta: Stock bajo - {stockData.NombreProducto}";
                string message = $"El producto '{stockData.NombreProducto}' tiene stock bajo: {stockData.CantidadActual} unidades. " +
                                $"Mínimo requerido: {stockData.StockMinimo}.";
                
                await _notificationService.SendAsync(destination, subject, message);
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
            else if (data is StockBajoEventData stockData)
            {
                _logger.LogWarning("⚠️ Stock bajo detectado: {Producto} - Cantidad: {Cantidad}/{Minimo}",
                    stockData.NombreProducto, stockData.CantidadActual, stockData.StockMinimo);
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

    public class StockBajoEventData
    {
        public int ProductoId { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public int CantidadActual { get; set; }
        public int StockMinimo { get; set; }
        public DateTime FechaDeteccion { get; set; }
    }
}
