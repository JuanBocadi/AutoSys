using AutoSys.Data;
using AutoSys.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace AutoSys.Services
{
    /// Gestiona permisos granulares por usuario. Usa BD si existe registro, sino defaults del rol.
    public class PermissionService : IPermissionService
    {
        private readonly AutoSysDbContext _context;
        private readonly ILogger<PermissionService> _logger;

        public PermissionService(AutoSysDbContext context, ILogger<PermissionService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ── Defaults por rol ─────────────────────────────────────────────
        public UserPermission GetDefaultsByRole(string rol, string userId)
        {
            return rol switch
            {
                "Administrador" => new UserPermission
                {
                    UserId = userId,
                    VerClientes = true,  CrearClientes = true,  EditarClientes = true,
                    VerVehiculos = true, CrearVehiculos = true, EditarVehiculos = true, TransferirVehiculo = true,
                    VerIngresos = true,  CrearIngresos = true,  ActualizarEstadoIngresos = true,
                    VerReparaciones = true,
                    VerStock = true,     CrearStock = true,  EditarStock = true,  AjustarStock = true,
                    VerFacturacion = true, CrearFacturas = true,
                    VerReportes = true,
                    VerBackups = true, GestionarBackups = true,
                    VerAuditoria = true,
                    VerServiciosFijos = true,
                    GestionarServiciosFijos = true
                },
                "Recepcionista" => new UserPermission
                {
                    UserId = userId,
                    VerClientes = true,  CrearClientes = true,  EditarClientes = true,
                    VerVehiculos = true, CrearVehiculos = true, EditarVehiculos = true, TransferirVehiculo = true,
                    VerIngresos = true,  CrearIngresos = true,  ActualizarEstadoIngresos = true,
                    VerReparaciones = false,
                    VerStock = false,    CrearStock = false, EditarStock = false, AjustarStock = false,
                    VerFacturacion = true, CrearFacturas = true,
                    VerReportes = false,
                    VerServiciosFijos = true,
                    GestionarServiciosFijos = true
                },
                "Mecanico" => new UserPermission
                {
                    UserId = userId,
                    VerClientes = false, CrearClientes = false, EditarClientes = false,
                    VerVehiculos = false, CrearVehiculos = false, EditarVehiculos = false,
                    VerIngresos = true,  CrearIngresos = false, ActualizarEstadoIngresos = false,
                    VerReparaciones = true,
                    VerStock = true,     CrearStock = false, EditarStock = false, AjustarStock = false,
                    VerFacturacion = false, CrearFacturas = false,
                    VerReportes = false
                },
                _ => new UserPermission { UserId = userId }
            };
        }

        // ── Permisos efectivos: usuario > rol (BD) > defaults hardcodeados ──
        public async Task<UserPermission> ObtenerPermisosEfectivosAsync(string userId, string rol)
        {
            // 1. Permisos individuales del usuario
            var guardados = await _context.UserPermissions
                .FirstOrDefaultAsync(p => p.UserId == userId);
            if (guardados != null)
                return guardados;

            // 2. Permisos configurados para el rol en BD
            var rolPermisos = await _context.RolePermissions
                .FirstOrDefaultAsync(p => p.RolNombre == rol);
            if (rolPermisos != null)
                return RolePermissionToUserPermission(rolPermisos, userId);

            // 3. Defaults hardcodeados del rol
            return GetDefaultsByRole(rol, userId);
        }

        private static UserPermission RolePermissionToUserPermission(RolePermission rp, string userId)
        {
            return new UserPermission
            {
                UserId                    = userId,
                VerClientes               = rp.VerClientes,
                CrearClientes             = rp.CrearClientes,
                EditarClientes            = rp.EditarClientes,
                VerVehiculos              = rp.VerVehiculos,
                CrearVehiculos            = rp.CrearVehiculos,
                EditarVehiculos           = rp.EditarVehiculos,
                TransferirVehiculo        = rp.TransferirVehiculo,
                VerIngresos               = rp.VerIngresos,
                CrearIngresos             = rp.CrearIngresos,
                ActualizarEstadoIngresos  = rp.ActualizarEstadoIngresos,
                VerReparaciones           = rp.VerReparaciones,
                VerStock                  = rp.VerStock,
                CrearStock                = rp.CrearStock,
                EditarStock               = rp.EditarStock,
                AjustarStock              = rp.AjustarStock,
                VerFacturacion            = rp.VerFacturacion,
                CrearFacturas             = rp.CrearFacturas,
                VerReportes               = rp.VerReportes,
                VerBackups                = rp.VerBackups,
                GestionarBackups          = rp.GestionarBackups,
                VerAuditoria              = rp.VerAuditoria,
                VerServiciosFijos         = rp.VerServiciosFijos,
                GestionarServiciosFijos   = rp.GestionarServiciosFijos,
            };
        }

        // ── Verificación individual ───────────────────────────────────────
        public async Task<bool> TienePermisoEfectivoAsync(string userId, string rol, string permiso)
        {
            var permisos = await ObtenerPermisosEfectivosAsync(userId, rol);

            var prop = typeof(UserPermission).GetProperty(permiso, BindingFlags.Public | BindingFlags.Instance);
            if (prop == null || prop.PropertyType != typeof(bool))
                return false;

            return (bool)(prop.GetValue(permisos) ?? false);
        }

        // ── Guardar ───────────────────────────────────────────────────────
        public async Task GuardarPermisosAsync(UserPermission permisos, string adminUsername)
        {
            permisos.UltimaModificacion = DateTime.Now;
            permisos.ModificadoPor = adminUsername;

            var existente = await _context.UserPermissions
                .FirstOrDefaultAsync(p => p.UserId == permisos.UserId);

            if (existente == null)
            {
                _context.UserPermissions.Add(permisos);
                _logger.LogInformation("Permisos creados para {UserId} por {Admin}",
                    permisos.UserId, adminUsername);
            }
            else
            {
                existente.VerClientes              = permisos.VerClientes;
                existente.CrearClientes            = permisos.CrearClientes;
                existente.EditarClientes           = permisos.EditarClientes;
                existente.VerVehiculos             = permisos.VerVehiculos;
                existente.CrearVehiculos           = permisos.CrearVehiculos;
                existente.EditarVehiculos          = permisos.EditarVehiculos;
                existente.TransferirVehiculo        = permisos.TransferirVehiculo;
                existente.VerIngresos              = permisos.VerIngresos;
                existente.CrearIngresos            = permisos.CrearIngresos;
                existente.ActualizarEstadoIngresos = permisos.ActualizarEstadoIngresos;
                existente.VerReparaciones          = permisos.VerReparaciones;
                existente.VerStock                 = permisos.VerStock;
                existente.CrearStock               = permisos.CrearStock;
                existente.EditarStock              = permisos.EditarStock;
                existente.AjustarStock             = permisos.AjustarStock;
                existente.VerFacturacion           = permisos.VerFacturacion;
                existente.CrearFacturas            = permisos.CrearFacturas;
                existente.VerReportes              = permisos.VerReportes;
                existente.VerBackups               = permisos.VerBackups;
                existente.GestionarBackups         = permisos.GestionarBackups;
                existente.VerAuditoria             = permisos.VerAuditoria;
                existente.VerServiciosFijos        = permisos.VerServiciosFijos;
                existente.GestionarServiciosFijos  = permisos.GestionarServiciosFijos;
                existente.UltimaModificacion       = permisos.UltimaModificacion;
                existente.ModificadoPor            = permisos.ModificadoPor;

                _logger.LogInformation("Permisos actualizados para {UserId} por {Admin}",
                    permisos.UserId, adminUsername);
            }

            await _context.SaveChangesAsync();
        }

        // ── Permisos de rol ───────────────────────────────────────────────
        public async Task<RolePermission?> ObtenerPermisosRolAsync(string rol)
        {
            return await _context.RolePermissions
                .FirstOrDefaultAsync(p => p.RolNombre == rol);
        }

        public async Task GuardarPermisosRolAsync(RolePermission permisos, string adminUsername)
        {
            permisos.UltimaModificacion = DateTime.Now;
            permisos.ModificadoPor = adminUsername;

            var existente = await _context.RolePermissions
                .FirstOrDefaultAsync(p => p.RolNombre == permisos.RolNombre);

            if (existente == null)
            {
                _context.RolePermissions.Add(permisos);
                _logger.LogInformation("Permisos de rol {Rol} creados por {Admin}",
                    permisos.RolNombre, adminUsername);
            }
            else
            {
                existente.VerClientes               = permisos.VerClientes;
                existente.CrearClientes             = permisos.CrearClientes;
                existente.EditarClientes            = permisos.EditarClientes;
                existente.VerVehiculos              = permisos.VerVehiculos;
                existente.CrearVehiculos            = permisos.CrearVehiculos;
                existente.EditarVehiculos           = permisos.EditarVehiculos;
                existente.TransferirVehiculo         = permisos.TransferirVehiculo;
                existente.VerIngresos               = permisos.VerIngresos;
                existente.CrearIngresos             = permisos.CrearIngresos;
                existente.ActualizarEstadoIngresos  = permisos.ActualizarEstadoIngresos;
                existente.VerReparaciones           = permisos.VerReparaciones;
                existente.VerStock                  = permisos.VerStock;
                existente.CrearStock                = permisos.CrearStock;
                existente.EditarStock               = permisos.EditarStock;
                existente.AjustarStock              = permisos.AjustarStock;
                existente.VerFacturacion            = permisos.VerFacturacion;
                existente.CrearFacturas             = permisos.CrearFacturas;
                existente.VerReportes               = permisos.VerReportes;
                existente.VerBackups                = permisos.VerBackups;
                existente.GestionarBackups          = permisos.GestionarBackups;
                existente.VerAuditoria              = permisos.VerAuditoria;
                existente.VerServiciosFijos         = permisos.VerServiciosFijos;
                existente.GestionarServiciosFijos   = permisos.GestionarServiciosFijos;
                existente.UltimaModificacion        = permisos.UltimaModificacion;
                existente.ModificadoPor             = permisos.ModificadoPor;
                _logger.LogInformation("Permisos de rol {Rol} actualizados por {Admin}",
                    permisos.RolNombre, adminUsername);
            }

            await _context.SaveChangesAsync();
        }

        // ── Eliminar overrides individuales (para propagar cambios de grupo) ──
        public async Task EliminarPermisosIndividualesAsync(IEnumerable<string> userIds)
        {
            var ids = userIds.ToList();
            if (!ids.Any()) return;

            var registros = await _context.UserPermissions
                .Where(p => ids.Contains(p.UserId))
                .ToListAsync();

            if (registros.Any())
            {
                _context.UserPermissions.RemoveRange(registros);
                await _context.SaveChangesAsync();
                _logger.LogInformation(
                    "Eliminados {Count} permisos individuales al propagar cambio de grupo.",
                    registros.Count);
            }
        }

        // ── Eliminar permisos de rol (al eliminar un grupo) ──
        public async Task EliminarPermisosRolAsync(string rolNombre)
        {
            var registro = await _context.RolePermissions
                .FirstOrDefaultAsync(p => p.RolNombre == rolNombre);

            if (registro != null)
            {
                _context.RolePermissions.Remove(registro);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Permisos del rol {Rol} eliminados de la BD.", rolNombre);
            }
        }
    }
}
