using AutoSys.Models;

namespace AutoSys.Services
{
    public interface IPermissionService
    {
        /// Verifica si el usuario puede hacer algo, considerando el rol base Y permisos delegados.
        /// Si el rol ya otorga ese permiso, retorna true sin consultar la BD.
        /// Si existe un registro en BD, usa ese valor (puede quitar permisos del rol o agregar).
        Task<bool> TienePermisoEfectivoAsync(string userId, string rol, string permiso);

        /// Obtiene los permisos efectivos del usuario.
        /// Si no hay registro en BD, retorna los defaults del rol.
        Task<UserPermission> ObtenerPermisosEfectivosAsync(string userId, string rol);

        /// Guarda o actualiza los permisos de un usuario.
        Task GuardarPermisosAsync(UserPermission permisos, string adminUsername);

        /// Retorna los permisos por defecto del rol indicado (sin consultar la BD).
        UserPermission GetDefaultsByRole(string rol, string userId);

        /// Obtiene los permisos configurados para un rol en la BD.
        /// Retorna null si nunca se personalizaron (se usan los defaults hardcodeados).
        Task<AutoSys.Models.RolePermission?> ObtenerPermisosRolAsync(string rol);

        /// Guarda o actualiza los permisos de un rol en la BD.
        Task GuardarPermisosRolAsync(AutoSys.Models.RolePermission permisos, string adminUsername);
        /// Elimina los registros de permisos individuales de los usuarios indicados,
        /// forzando que hereden los permisos del grupo.
        Task EliminarPermisosIndividualesAsync(IEnumerable<string> userIds);

        /// Elimina los permisos configurados de un rol en la BD (al eliminar un grupo).
        Task EliminarPermisosRolAsync(string rolNombre);
    }
}
