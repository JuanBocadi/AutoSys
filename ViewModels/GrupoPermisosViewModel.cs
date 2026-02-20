using AutoSys.Models;

namespace AutoSys.ViewModels
{
    /// <summary>
    /// ViewModel para la página de permisos de grupos (roles Mecanico y Recepcionista).
    /// </summary>
    public class GrupoPermisosViewModel
    {
        public RolePermission PermisosMecanico { get; set; } = new() { RolNombre = "Mecanico" };
        public RolePermission PermisosRecepcionista { get; set; } = new() { RolNombre = "Recepcionista" };

        // Defaults hardcodeados del sistema (para la leyenda comparativa)
        public UserPermission DefaultsMecanico { get; set; } = new();
        public UserPermission DefaultsRecepcionista { get; set; } = new();

        // Fecha de última modificación de cada rol (null si nunca se guardaron en BD)
        public DateTime? UltimaModifMecanico { get; set; }
        public DateTime? UltimaModifRecepcionista { get; set; }
        public string? ModifPorMecanico { get; set; }
        public string? ModifPorRecepcionista { get; set; }
    }
}
