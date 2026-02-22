using AutoSys.Models;

namespace AutoSys.Services
{
    public interface IPermissionService
    {
        /// <summary>
        /// Verifica si el usuario puede hacer algo, considerando el rol base Y permisos delegados.
        /// Si el rol ya otorga ese permiso, retorna true sin consultar la BD.
        /// Si existe un registro en BD, usa ese valor (puede quitar permisos del rol o agregar).
        /// </summary>
        Task<bool> TienePermisoEfectivoAsync(string userId, string rol, string permiso);

        /// <summary>
        /// Obtiene los permisos efectivos del usuario.
        /// Si no hay registro en BD, retorna los defaults del rol.
        /// </summary>
        Task<UserPermission> ObtenerPermisosEfectivosAsync(string userId, string rol);

        /// <summary>
        /// Guarda o actualiza los permisos de un usuario.
        /// </summary>
        Task GuardarPermisosAsync(UserPermission permisos, string adminUsername);

        /// <summary>
        /// Retorna los permisos por defecto del rol indicado (sin consultar la BD).
        /// </summary>
        UserPermission GetDefaultsByRole(string rol, string userId);

        /// <summary>
        /// Obtiene los permisos configurados para un rol en la BD.
        /// Retorna null si nunca se personalizaron (se usan los defaults hardcodeados).
        /// </summary>
        Task<AutoSys.Models.RolePermission?> ObtenerPermisosRolAsync(string rol);

        /// <summary>
        /// Guarda o actualiza los permisos de un rol en la BD.
        /// </summary>
        Task GuardarPermisosRolAsync(AutoSys.Models.RolePermission permisos, string adminUsername);
        /// <summary>
        /// Elimina los registros de permisos individuales de los usuarios indicados,
        /// forzando que hereden los permisos del grupo.
        /// </summary>
        Task EliminarPermisosIndividualesAsync(IEnumerable<string> userIds);

        /// <summary>
        /// Elimina los permisos configurados de un rol en la BD (al eliminar un grupo).
        /// </summary>
        Task EliminarPermisosRolAsync(string rolNombre);
    }
}
