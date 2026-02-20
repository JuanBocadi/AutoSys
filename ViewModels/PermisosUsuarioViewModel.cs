using AutoSys.Models;

namespace AutoSys.ViewModels
{
    public class PermisosUsuarioViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        /// <summary>Rol actual del usuario</summary>
        public string Rol { get; set; } = string.Empty;

        /// <summary>Nuevo rol seleccionado por el administrador</summary>
        public string NuevoRol { get; set; } = string.Empty;

        /// <summary>Permisos efectivos (guardados en BD o defaults del rol)</summary>
        public UserPermission Permisos { get; set; } = new UserPermission();

        /// <summary>Permisos que el rol actual da por defecto (para mostrar etiquetas informativas)</summary>
        public UserPermission DefaultsDelRol { get; set; } = new UserPermission();

        /// <summary>Roles disponibles para cambiar</summary>
        public List<string> RolesDisponibles { get; set; } = new List<string>();
    }
}
