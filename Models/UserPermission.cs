using System.ComponentModel.DataAnnotations;

namespace AutoSys.Models
{
    /// <summary>
    /// Permisos granulares por usuario, independientes del rol base.
    /// Permiten al administrador delegar funcionalidades específicas.
    /// </summary>
    public class UserPermission
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

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

        public DateTime UltimaModificacion { get; set; } = DateTime.Now;
        public string ModificadoPor { get; set; } = string.Empty;
    }
}
