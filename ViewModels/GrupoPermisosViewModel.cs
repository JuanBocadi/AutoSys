using AutoSys.Models;

namespace AutoSys.ViewModels
{
    /// <summary>
    /// ViewModel para la página de permisos de grupos (roles dinámicos).
    /// </summary>
    public class GrupoPermisosViewModel
    {
        /// <summary>Lista dinámica de grupos con sus permisos.</summary>
        public List<GrupoPermisoItem> Grupos { get; set; } = new();
    }

    /// <summary>
    /// Representa un grupo/rol con sus permisos configurados.
    /// </summary>
    public class GrupoPermisoItem
    {
        public string RolNombre { get; set; } = string.Empty;
        public RolePermission Permisos { get; set; } = new();
        public UserPermission Defaults { get; set; } = new();
        public DateTime? UltimaModificacion { get; set; }
        public string? ModificadoPor { get; set; }
        public int CantidadUsuarios { get; set; }

        /// <summary>Indica si es un rol predeterminado del sistema (no eliminable).</summary>
        public bool EsRolSistema { get; set; }
    }
}
