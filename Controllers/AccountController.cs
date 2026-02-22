using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AutoSys.ViewModels;
using AutoSys.Patterns.Composite;
using AutoSys.Patterns.Singleton;
using AutoSys.Services;
using System.Threading.Tasks;
using System.Linq;

namespace AutoSys.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IAuditService _auditService;

        public AccountController(UserManager<IdentityUser> userManager,
                                 SignInManager<IdentityUser> signInManager,
                                 RoleManager<IdentityRole> roleManager,
                                 IAuditService auditService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _auditService = auditService;
        }

        [HttpGet]
        [Authorize(Roles = "Administrador")]
        public IActionResult Register()
        {
            ViewBag.Roles = _roleManager.Roles.Select(r => r.Name).ToList();
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            // PATRÓN COMPOSITE: Validación jerárquica de registro
            var validationContext = new ValidationContext
            {
                Username = model.Username,
                Email = model.Email,
                Password = model.Password,
                Rol = model.Rol
            };

            var validator = ValidationFactory.CreateRegisterValidation();
            if (!validator.Validate(validationContext))
            {
                ModelState.AddModelError(string.Empty, validator.GetErrorMessage());
                ViewBag.Roles = _roleManager.Roles.Select(r => r.Name).ToList();
                return View(model);
            }

            if (ModelState.IsValid)
            {
                var user = new IdentityUser { UserName = model.Username, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    if (await _roleManager.RoleExistsAsync(model.Rol))
                    {
                        await _userManager.AddToRoleAsync(user, model.Rol);
                    }

                    TempData["Success"] = "Usuario registrado correctamente.";

                    // AUDITORÍA
                    await _auditService.RegistrarAsync(User.Identity?.Name ?? "desconocido", "Administrador", "Usuario", "Crear",
                        $"Usuario registrado: {model.Username} (Rol: {model.Rol})",
                        null, model.Username, HttpContext.Connection.RemoteIpAddress?.ToString());

                    return RedirectToAction("Index", "Usuarios");
                }

                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
            }

            ViewBag.Roles = _roleManager.Roles.Select(r => r.Name).ToList();
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password)
        {
            // PATRÓN COMPOSITE: Validación jerárquica de login
            // PATRÓN SINGLETON: Obtener configuración de longitud mínima de contraseña
            var config = AppConfigurationManager.Instance;
            int minPasswordLength = config.GetSettingAsInt("MinPasswordLength", 6);

            var validationContext = new ValidationContext
            {
                Username = email,
                Password = password
            };

            var validator = ValidationFactory.CreateLoginValidation();
            if (!validator.Validate(validationContext))
            {
                ViewBag.Error = validator.GetErrorMessage();
                return View();
            }

            var user = await _userManager.FindByNameAsync(email) ?? await _userManager.FindByEmailAsync(email);
            
            if (user != null)
            {
                var result = await _signInManager.PasswordSignInAsync(
                    user.UserName!, password,
                    isPersistent: false,
                    lockoutOnFailure: true);  // activa bloqueo tras 5 intentos fallidos
                
                if (result.Succeeded)
                {
                    // AUDITORÍA
                    var roles = await _userManager.GetRolesAsync(user);
                    var rol = roles.FirstOrDefault() ?? "Sin rol";
                    await _auditService.RegistrarAsync(user.UserName ?? email, rol, "Sesion", "Login",
                        $"Inicio de sesión exitoso",
                        null, user.UserName, HttpContext.Connection.RemoteIpAddress?.ToString());

                    return RedirectToAction("Index", "Home");
                }

                if (result.IsLockedOut)
                {
                    ViewBag.Error = "Cuenta bloqueada temporalmente por múltiples intentos fallidos. Intente en unos minutos.";
                    return View();
                }
            }

            ViewBag.Error = "Usuario o contraseña incorrectos.";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // AUDITORÍA
            var rolLogout = User.IsInRole("Administrador") ? "Administrador" : User.IsInRole("Recepcionista") ? "Recepcionista" : "Mecanico";
            await _auditService.RegistrarAsync(User.Identity?.Name ?? "desconocido", rolLogout, "Sesion", "Logout",
                "Cierre de sesión",
                null, User.Identity?.Name, HttpContext.Connection.RemoteIpAddress?.ToString());

            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }

        // ──────────────────────────────────────────────────────────────
        // CAMBIAR CONTRASEÑA (usuario autenticado cambia su propia clave)
        // ──────────────────────────────────────────────────────────────

        [HttpGet]
        [Authorize]
        public IActionResult CambiarPassword()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarPassword(string passwordActual, string nuevaPassword, string confirmarPassword)
        {
            if (string.IsNullOrWhiteSpace(passwordActual) || string.IsNullOrWhiteSpace(nuevaPassword))
            {
                ViewBag.Error = "Todos los campos son obligatorios.";
                return View();
            }

            if (nuevaPassword.Length < 8)
            {
                ViewBag.Error = "La nueva contraseña debe tener al menos 8 caracteres.";
                return View();
            }

            if (nuevaPassword != confirmarPassword)
            {
                ViewBag.Error = "Las contraseñas no coinciden.";
                return View();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login");

            var result = await _userManager.ChangePasswordAsync(user, passwordActual, nuevaPassword);
            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);
                TempData["SuccessMessage"] = "Contraseña actualizada correctamente.";
                return RedirectToAction("Index", "Home");
            }

            var errores = result.Errors.Select(e => e.Description).ToList();
            if (errores.Any(e => e.Contains("Incorrect password")))
                ViewBag.Error = "La contraseña actual es incorrecta.";
            else
                ViewBag.Error = string.Join(" ", errores);

            return View();
        }

        // ──────────────────────────────────────────────────────────────
        // RECUPERAR CONTRASEÑA (usuario anónimo recupera su clave)
        // ──────────────────────────────────────────────────────────────

        [HttpGet]
        [AllowAnonymous]
        public IActionResult RecuperarPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecuperarPassword(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                ViewBag.Error = "Debe ingresar su correo electrónico o nombre de usuario.";
                return View();
            }

            var user = await _userManager.FindByEmailAsync(email)
                    ?? await _userManager.FindByNameAsync(email);

            if (user == null)
            {
                ViewBag.Error = "No se encontró una cuenta asociada a esos datos. Contacte al administrador.";
                return View();
            }

            // Generar token de restablecimiento
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            // En un entorno de producción se enviaría por email.
            // Al ser un sistema local, redirigimos directamente al formulario de reset.
            return RedirectToAction("RestablecerConToken", new { userId = user.Id, token });
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> RestablecerConToken(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                return RedirectToAction("Login");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return RedirectToAction("Login");

            ViewBag.UserId = userId;
            ViewBag.Token = token;
            ViewBag.UserName = user.UserName;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestablecerConToken(string userId, string token, string nuevaPassword, string confirmarPassword)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                return RedirectToAction("Login");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return RedirectToAction("Login");

            if (string.IsNullOrWhiteSpace(nuevaPassword) || nuevaPassword.Length < 8)
            {
                ViewBag.Error = "La contraseña debe tener al menos 8 caracteres.";
                ViewBag.UserId = userId;
                ViewBag.Token = token;
                ViewBag.UserName = user.UserName;
                return View();
            }

            if (nuevaPassword != confirmarPassword)
            {
                ViewBag.Error = "Las contraseñas no coinciden.";
                ViewBag.UserId = userId;
                ViewBag.Token = token;
                ViewBag.UserName = user.UserName;
                return View();
            }

            var result = await _userManager.ResetPasswordAsync(user, token, nuevaPassword);
            if (result.Succeeded)
            {
                TempData["Success"] = "Contraseña restablecida correctamente. Inicie sesión con su nueva contraseña.";
                return RedirectToAction("Login");
            }

            ViewBag.Error = string.Join(" ", result.Errors.Select(e => e.Description));
            ViewBag.UserId = userId;
            ViewBag.Token = token;
            ViewBag.UserName = user.UserName;
            return View();
        }
    }
}
