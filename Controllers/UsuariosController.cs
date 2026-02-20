using AutoSys.Models;
using AutoSys.Services;
using AutoSys.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutoSys.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class UsuariosController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IPermissionService _permissionService;
        private readonly ILogger<UsuariosController> _logger;

        public UsuariosController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IPermissionService permissionService,
            ILogger<UsuariosController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _permissionService = permissionService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var userViewModels = new List<UsuarioViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userViewModels.Add(new UsuarioViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    Rol = roles.FirstOrDefault() ?? "Sin rol" // Mostramos el primer rol, o texto por defecto
                });
            }

            return View(userViewModels);
        }

        [HttpGet]
        public async Task<IActionResult> GetUserDetails(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            var userDetails = new UsuarioViewModel
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                Rol = roles.FirstOrDefault() ?? "Sin rol"
            };

            return Json(userDetails);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser != null && currentUser.Id == id)
            {
                TempData["ErrorMessage"] = "No puede eliminar su propia cuenta.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = "No se pudo eliminar el usuario.";
            }
            else
            {
                TempData["SuccessMessage"] = "Usuario eliminado correctamente.";
            }

            return RedirectToAction(nameof(Index));
        }

        // ──────────────────────────────────────────────────────────────
        // VERIFICACIÓN DE CONTRASEÑA (AJAX)
        // ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Verifica la contraseña del administrador activo antes de mostrar permisos.
        /// Retorna JSON { success: true/false }.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerificarPassword([FromBody] VerificarPasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Password))
                return Json(new { success = false, mensaje = "La contraseña no puede estar vacía." });

            var adminUser = await _userManager.GetUserAsync(User);
            if (adminUser == null)
                return Json(new { success = false, mensaje = "No se pudo identificar al administrador." });

            var passwordOk = await _userManager.CheckPasswordAsync(adminUser, request.Password);

            if (!passwordOk)
            {
                _logger.LogWarning("Verificación de contraseña fallida para administrador {Admin}",
                    adminUser.UserName);
                return Json(new { success = false, mensaje = "Contraseña incorrecta." });
            }

            _logger.LogInformation("Administrador {Admin} verificó identidad para gestión de permisos.",
                adminUser.UserName);

            return Json(new { success = true });
        }

        // ──────────────────────────────────────────────────────────────
        // GESTIÓN DE PERMISOS
        // ──────────────────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> Permisos(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var targetUser = await _userManager.FindByIdAsync(id);
            if (targetUser == null)
                return NotFound();

            var roles = await _userManager.GetRolesAsync(targetUser);
            var rol = roles.FirstOrDefault() ?? "Sin rol";

            // Si no hay registro en BD, pre-cargamos los defaults del rol actual
            var permisos = await _permissionService.ObtenerPermisosEfectivosAsync(id, rol);
            permisos.UserId = id;

            var vm = new PermisosUsuarioViewModel
            {
                UserId = id,
                UserName = targetUser.UserName ?? string.Empty,
                Email = targetUser.Email ?? string.Empty,
                Rol = rol,
                NuevoRol = rol,
                Permisos = permisos,
                RolesDisponibles = new List<string> { "Administrador", "Recepcionista", "Mecanico" },
                DefaultsDelRol = _permissionService.GetDefaultsByRole(rol, id)
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarPermisos(string userId, string nuevoRol, UserPermission permisos)
        {
            if (string.IsNullOrEmpty(userId))
                return NotFound();

            var targetUser = await _userManager.FindByIdAsync(userId);
            if (targetUser == null)
                return NotFound();

            // No se pueden modificar permisos del propio administrador
            var adminUser = await _userManager.GetUserAsync(User);
            if (adminUser != null && adminUser.Id == userId)
            {
                TempData["ErrorMessage"] = "No puede modificar los permisos de su propia cuenta.";
                return RedirectToAction(nameof(Permisos), new { id = userId });
            }

            // ── Cambio de rol base ────────────────────────────────────────
            var rolesValidos = new[] { "Administrador", "Recepcionista", "Mecanico" };
            if (!string.IsNullOrEmpty(nuevoRol) && rolesValidos.Contains(nuevoRol))
            {
                var rolesActuales = await _userManager.GetRolesAsync(targetUser);
                var rolActual = rolesActuales.FirstOrDefault();

                if (rolActual != nuevoRol)
                {
                    if (!string.IsNullOrEmpty(rolActual))
                        await _userManager.RemoveFromRoleAsync(targetUser, rolActual);

                    await _userManager.AddToRoleAsync(targetUser, nuevoRol);

                    _logger.LogInformation("Rol de {User} cambiado de {RolOld} a {RolNew} por {Admin}",
                        targetUser.UserName, rolActual, nuevoRol, adminUser?.UserName);
                }
            }

            // ── Guardar permisos granulares ───────────────────────────────
            permisos.UserId = userId;
            await _permissionService.GuardarPermisosAsync(permisos, adminUser?.UserName ?? "desconocido");

            _logger.LogInformation("Permisos actualizados para {TargetUser} por {Admin}",
                targetUser.UserName, adminUser?.UserName);

            TempData["SuccessMessage"] = $"Permisos y rol de {targetUser.UserName} actualizados correctamente.";
            return RedirectToAction(nameof(Permisos), new { id = userId });
        }

        // ──────────────────────────────────────────────────────────────
        // PERMISOS DE GRUPOS (roles Mecanico / Recepcionista)
        // ──────────────────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> GrupoPermisos()
        {
            var mecDB  = await _permissionService.ObtenerPermisosRolAsync("Mecanico");
            var recDB  = await _permissionService.ObtenerPermisosRolAsync("Recepcionista");

            var vm = new AutoSys.ViewModels.GrupoPermisosViewModel
            {
                PermisosMecanico      = mecDB  ?? ToRolePermission(_permissionService.GetDefaultsByRole("Mecanico", ""),      "Mecanico"),
                PermisosRecepcionista = recDB  ?? ToRolePermission(_permissionService.GetDefaultsByRole("Recepcionista", ""), "Recepcionista"),
                DefaultsMecanico      = _permissionService.GetDefaultsByRole("Mecanico", ""),
                DefaultsRecepcionista = _permissionService.GetDefaultsByRole("Recepcionista", ""),
                UltimaModifMecanico      = mecDB?.UltimaModificacion,
                UltimaModifRecepcionista = recDB?.UltimaModificacion,
                ModifPorMecanico         = mecDB?.ModificadoPor,
                ModifPorRecepcionista    = recDB?.ModificadoPor,
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarPermisosGrupo(string rolNombre, AutoSys.Models.RolePermission permisos)
        {
            var rolesValidos = new[] { "Mecanico", "Recepcionista" };
            if (!rolesValidos.Contains(rolNombre))
            {
                TempData["ErrorMessage"] = "Rol no válido.";
                return RedirectToAction(nameof(GrupoPermisos));
            }

            var adminUser = await _userManager.GetUserAsync(User);
            permisos.RolNombre = rolNombre;
            await _permissionService.GuardarPermisosRolAsync(permisos, adminUser?.UserName ?? "desconocido");

            // Propagar a todos los usuarios del grupo: eliminar sus overrides individuales
            // para que hereden la nueva configuración del grupo.
            var usersEnRol = await _userManager.GetUsersInRoleAsync(rolNombre);
            var userIds = usersEnRol.Select(u => u.Id).ToList();
            await _permissionService.EliminarPermisosIndividualesAsync(userIds);

            _logger.LogInformation("Permisos del grupo {Rol} actualizados y propagados a {Count} usuarios por {Admin}",
                rolNombre, userIds.Count, adminUser?.UserName);
            TempData["SuccessMessage"] = $"Permisos del grupo \"{rolNombre}\" actualizados y aplicados a {userIds.Count} usuario(s) del grupo.";
            return RedirectToAction(nameof(GrupoPermisos));
        }

        private static AutoSys.Models.RolePermission ToRolePermission(AutoSys.Models.UserPermission up, string rolNombre) =>
            new()
            {
                RolNombre                = rolNombre,
                VerClientes              = up.VerClientes,
                CrearClientes            = up.CrearClientes,
                EditarClientes           = up.EditarClientes,
                VerVehiculos             = up.VerVehiculos,
                CrearVehiculos           = up.CrearVehiculos,
                VerIngresos              = up.VerIngresos,
                CrearIngresos            = up.CrearIngresos,
                ActualizarEstadoIngresos = up.ActualizarEstadoIngresos,
                VerReparaciones          = up.VerReparaciones,
                VerStock                 = up.VerStock,
                CrearStock               = up.CrearStock,
                EditarStock              = up.EditarStock,
                AjustarStock             = up.AjustarStock,
                VerFacturacion           = up.VerFacturacion,
                CrearFacturas            = up.CrearFacturas,
                VerReportes              = up.VerReportes,
            };

        // ──────────────────────────────────────────────────────────────
        // RESTABLECIMIENTO DE CONTRASEÑA
        // ──────────────────────────────────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestablecerPassword(string userId, string nuevaPassword, string confirmarPassword)
        {
            if (string.IsNullOrEmpty(userId))
                return NotFound();

            // No puede resetear su propia contraseña desde aquí
            var adminUser = await _userManager.GetUserAsync(User);
            if (adminUser != null && adminUser.Id == userId)
            {
                TempData["ErrorMessage"] = "No puede restablecer su propia contraseña desde este panel.";
                return RedirectToAction(nameof(Permisos), new { id = userId });
            }

            if (string.IsNullOrWhiteSpace(nuevaPassword) || nuevaPassword.Length < 6)
            {
                TempData["ErrorMessage"] = "La contraseña debe tener al menos 6 caracteres.";
                return RedirectToAction(nameof(Permisos), new { id = userId });
            }

            if (nuevaPassword != confirmarPassword)
            {
                TempData["ErrorMessage"] = "Las contraseñas no coinciden.";
                return RedirectToAction(nameof(Permisos), new { id = userId });
            }

            var targetUser = await _userManager.FindByIdAsync(userId);
            if (targetUser == null)
                return NotFound();

            var removeResult = await _userManager.RemovePasswordAsync(targetUser);
            if (!removeResult.Succeeded)
            {
                TempData["ErrorMessage"] = "Error al restablecer la contraseña.";
                return RedirectToAction(nameof(Permisos), new { id = userId });
            }

            var addResult = await _userManager.AddPasswordAsync(targetUser, nuevaPassword);
            if (addResult.Succeeded)
            {
                _logger.LogWarning("Contraseña restablecida para {User} por administrador {Admin}",
                    targetUser.UserName, adminUser?.UserName);
                TempData["SuccessMessage"] = $"Contraseña de {targetUser.UserName} restablecida correctamente.";
            }
            else
            {
                var errores = string.Join(" ", addResult.Errors.Select(e => e.Description));
                TempData["ErrorMessage"] = $"No se pudo establecer la contraseña: {errores}";
            }

            return RedirectToAction(nameof(Permisos), new { id = userId });
        }

        // ──────────────────────────────────────────────────────────────
    }

    // DTO para la verificación de contraseña por AJAX
    public class VerificarPasswordRequest
    {
        public string Password { get; set; } = string.Empty;
    }
}