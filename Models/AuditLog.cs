using System.ComponentModel.DataAnnotations;

namespace AutoSys.Models
{
    /// <summary>
    /// Registro de auditoría para acciones importantes del sistema.
    /// Permite trazabilidad completa de quién hizo qué y cuándo.
    /// </summary>
    public class AuditLog
    {
        public int Id { get; set; }

        /// <summary>Fecha y hora de la acción</summary>
        public DateTime Fecha { get; set; } = DateTime.Now;

        /// <summary>Usuario que realizó la acción (username)</summary>
        [Required]
        [MaxLength(256)]
        public string Usuario { get; set; } = string.Empty;

        /// <summary>Rol del usuario al momento de la acción</summary>
        [MaxLength(50)]
        public string Rol { get; set; } = string.Empty;

        /// <summary>Categoría de la acción: Cliente, Vehiculo, Ingreso, Stock, Factura, Usuario, Permiso, Backup</summary>
        [Required]
        [MaxLength(50)]
        public string Categoria { get; set; } = string.Empty;

        /// <summary>Tipo de acción: Crear, Editar, Eliminar, CambioEstado, Login, Logout, etc.</summary>
        [Required]
        [MaxLength(50)]
        public string Accion { get; set; } = string.Empty;

        /// <summary>Descripción legible de lo que se hizo</summary>
        [Required]
        [MaxLength(500)]
        public string Descripcion { get; set; } = string.Empty;

        /// <summary>ID de la entidad afectada (si aplica)</summary>
        public int? EntidadId { get; set; }

        /// <summary>Nombre/identificador de la entidad afectada (ej: patente, nombre cliente)</summary>
        [MaxLength(200)]
        public string? EntidadNombre { get; set; }

        /// <summary>Dirección IP del usuario</summary>
        [MaxLength(50)]
        public string? DireccionIP { get; set; }
    }
}
