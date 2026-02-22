using AutoSys.Models;

namespace AutoSys.Services
{
    /// <summary>
    /// Interfaz para el servicio de auditoría.
    /// Registra acciones importantes realizadas por los usuarios del sistema.
    /// </summary>
    public interface IAuditService
    {
        /// <summary>Registra una acción de auditoría</summary>
        Task RegistrarAsync(string usuario, string rol, string categoria, string accion,
                            string descripcion, int? entidadId = null, string? entidadNombre = null,
                            string? direccionIP = null);

        /// <summary>Obtiene los registros de auditoría con filtros opcionales</summary>
        Task<List<AuditLog>> ObtenerLogsAsync(DateTime? desde = null, DateTime? hasta = null,
                                               string? categoria = null, string? usuario = null,
                                               int cantidad = 200);

        /// <summary>Obtiene las categorías disponibles</summary>
        Task<List<string>> ObtenerCategoriasAsync();
    }
}
