using AutoSys.Models;

namespace AutoSys.Services
{
    /// Interfaz del servicio de auditoría - registra acciones importantes de los usuarios
    public interface IAuditService
    {
        /// Registra una acción de auditoría
        Task RegistrarAsync(string usuario, string rol, string categoria, string accion,
                            string descripcion, int? entidadId = null, string? entidadNombre = null,
                            string? direccionIP = null);

        /// Obtiene los registros de auditoría con filtros opcionales
        Task<List<AuditLog>> ObtenerLogsAsync(DateTime? desde = null, DateTime? hasta = null,
                                               string? categoria = null, string? usuario = null,
                                               int cantidad = 200);

        /// Obtiene las categorías disponibles
        Task<List<string>> ObtenerCategoriasAsync();
    }
}
