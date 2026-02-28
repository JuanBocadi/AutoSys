using System.ComponentModel.DataAnnotations;

namespace AutoSys.Models
{
    /// Permisos por defecto a nivel de rol, aplican a usuarios sin permisos individuales
    public class RolePermission
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string RolNombre { get; set; } = string.Empty;

        // ── CLIENTES ──────────────────────────────────────────
        public bool VerClientes { get; set; } = false;
        public bool CrearClientes { get; set; } = false;
        public bool EditarClientes { get; set; } = false;

        // ── VEHÍCULOS ─────────────────────────────────────────
        public bool VerVehiculos { get; set; } = false;
        public bool CrearVehiculos { get; set; } = false;
        public bool EditarVehiculos { get; set; } = false;

        // ── INGRESOS ──────────────────────────────────────────
        public bool VerIngresos { get; set; } = false;
        public bool CrearIngresos { get; set; } = false;
        public bool ActualizarEstadoIngresos { get; set; } = false;

        // ── REPARACIONES ──────────────────────────────────────
        public bool VerReparaciones { get; set; } = false;

        // ── STOCK ─────────────────────────────────────────────
        public bool VerStock { get; set; } = false;
        public bool CrearStock { get; set; } = false;
        public bool EditarStock { get; set; } = false;
        public bool AjustarStock { get; set; } = false;

        // ── FACTURACIÓN ───────────────────────────────────────
        public bool VerFacturacion { get; set; } = false;
        public bool CrearFacturas { get; set; } = false;

        // ── REPORTES ──────────────────────────────────────────
        public bool VerReportes { get; set; } = false;

        // ── BACKUPS ───────────────────────────────────────────
        public bool VerBackups { get; set; } = false;
        public bool GestionarBackups { get; set; } = false;

        // ── AUDITORÍA ─────────────────────────────────────────
        public bool VerAuditoria { get; set; } = false;

        // ── SERVICIOS FIJOS ───────────────────────────────────
        public bool VerServiciosFijos { get; set; } = false;
        public bool GestionarServiciosFijos { get; set; } = false;

        public DateTime UltimaModificacion { get; set; } = DateTime.Now;
        public string ModificadoPor { get; set; } = string.Empty;
    }
}
