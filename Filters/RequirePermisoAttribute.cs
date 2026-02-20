using AutoSys.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace AutoSys.Filters
{
    /// <summary>
    /// Verifica que el usuario tenga el permiso granular requerido (via IPermissionService).
    /// Admin siempre pasa. Recepcionista y Mecánico consultan la BD / defaults del rol.
    /// Uso: [RequirePermiso("NombrePermiso")]
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RequirePermisoAttribute : TypeFilterAttribute
    {
        public RequirePermisoAttribute(string permiso) : base(typeof(PermisoAuthorizationFilter))
        {
            Arguments = new object[] { permiso };
        }
    }

    public class PermisoAuthorizationFilter : IAsyncAuthorizationFilter
    {
        private readonly IPermissionService _permissionService;
        private readonly string _permiso;

        public PermisoAuthorizationFilter(IPermissionService permissionService, string permiso)
        {
            _permissionService = permissionService;
            _permiso = permiso;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            // No autenticado → el [Authorize] del controller ya maneja esto
            if (!(user.Identity?.IsAuthenticated ?? false)) return;

            // Administrador siempre tiene acceso total
            if (user.IsInRole("Administrador")) return;

            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var rol = user.IsInRole("Recepcionista") ? "Recepcionista" : "Mecanico";

            var allowed = await _permissionService.TienePermisoEfectivoAsync(userId, rol, _permiso);
            if (!allowed)
            {
                context.Result = new ForbidResult();
            }
        }
    }
}
