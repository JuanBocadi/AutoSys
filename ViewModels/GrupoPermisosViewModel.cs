using AutoSys.Models;

namespace AutoSys.ViewModels
{
    /// ViewModel para la página de permisos de grupos/roles
    public class GrupoPermisosViewModel
    {
        /// Lista dinámica de grupos con sus permisos
        public List<GrupoPermisoItem> Grupos { get; set; } = new();
    }

    /// Representa un grupo/rol con sus permisos configurados
    public class GrupoPermisoItem
    {
        public string RolNombre { get; set; } = string.Empty;
        public RolePermission Permisos { get; set; } = new();
        public UserPermission Defaults { get; set; } = new();
        public DateTime? UltimaModificacion { get; set; }
        public string? ModificadoPor { get; set; }
        public int CantidadUsuarios { get; set; }

        /// Indica si es un rol predeterminado del sistema (no eliminable)
        public bool EsRolSistema { get; set; }
    }
}
