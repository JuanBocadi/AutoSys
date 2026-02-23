using System.ComponentModel.DataAnnotations;

namespace AutoSys.Models
{
    /// Registro de auditoría para trazabilidad de acciones del sistema
    public class AuditLog
    {
        public int Id { get; set; }

        /// Fecha y hora de la acción
        public DateTime Fecha { get; set; } = DateTime.Now;

        /// Usuario que realizó la acción
        [Required]
        [MaxLength(256)]
        public string Usuario { get; set; } = string.Empty;

        /// Rol del usuario al momento de la acción
        [MaxLength(50)]
        public string Rol { get; set; } = string.Empty;

        /// Categoría: Cliente, Vehiculo, Ingreso, Stock, Factura, Usuario, Permiso, Backup
        [Required]
        [MaxLength(50)]
        public string Categoria { get; set; } = string.Empty;

        /// Tipo de acción: Crear, Editar, Eliminar, CambioEstado, Login, Logout, etc.
        [Required]
        [MaxLength(50)]
        public string Accion { get; set; } = string.Empty;

        /// Descripción legible de lo que se hizo
        [Required]
        [MaxLength(500)]
        public string Descripcion { get; set; } = string.Empty;

        /// ID de la entidad afectada (si aplica)
        public int? EntidadId { get; set; }

        /// Nombre/identificador de la entidad afectada
        [MaxLength(200)]
        public string? EntidadNombre { get; set; }

        /// Dirección IP del usuario
        [MaxLength(50)]
        public string? DireccionIP { get; set; }
    }
}
