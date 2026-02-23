using AutoSys.Models;

namespace AutoSys.ViewModels
{
    public class PermisosUsuarioViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        /// Rol actual del usuario
        public string Rol { get; set; } = string.Empty;

        /// Nuevo rol seleccionado por el administrador
        public string NuevoRol { get; set; } = string.Empty;

        /// Permisos efectivos (guardados en BD o defaults del rol)
        public UserPermission Permisos { get; set; } = new UserPermission();

        /// Permisos por defecto del rol actual (para etiquetas informativas)
        public UserPermission DefaultsDelRol { get; set; } = new UserPermission();

        /// Roles disponibles para cambiar
        public List<string> RolesDisponibles { get; set; } = new List<string>();
    }
}
