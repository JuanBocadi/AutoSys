using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AutoSys.ViewModels;
using AutoSys.Filters;
using AutoSys.Patterns.Composite;
using AutoSys.Patterns.Singleton;
using AutoSys.Services;
using System.Threading.Tasks;
using System.Linq;
using System.Web;

namespace AutoSys.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IAuditService _auditService;
        private readonly IEmailService _emailService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(UserManager<IdentityUser> userManager,
                                 SignInManager<IdentityUser> signInManager,
                                 RoleManager<IdentityRole> roleManager,
                                 IAuditService auditService,
                                 IEmailService emailService,
                                 ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _auditService = auditService;
            _emailService = emailService;
            _logger = logger;
        }

        [HttpGet]
        [RequirePermiso("GestionarUsuarios")]
        public IActionResult Register()
        {
            ViewBag.Roles = _roleManager.Roles.Select(r => r.Name).ToList();
            return View();
        }

        [HttpPost]
        [RequirePermiso("GestionarUsuarios")]
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
                ViewBag.Error = "Debe ingresar su correo electrónico.";
                return View();
            }

            // Solo buscar por email (más seguro que aceptar también username)
            var user = await _userManager.FindByEmailAsync(email);

            if (user != null && !string.IsNullOrEmpty(user.Email))
            {
                try
                {
                    // Generar token de restablecimiento
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);

                    // Construir enlace de restablecimiento
                    var resetLink = Url.Action(
                        "RestablecerConToken",
                        "Account",
                        new { userId = user.Id, token },
                        protocol: Request.Scheme);

                    // Enviar correo con el enlace
                    var htmlBody = $@"
                        <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
                            <div style='background: linear-gradient(135deg, #1e40af, #3b82f6); padding: 30px; border-radius: 10px 10px 0 0; text-align: center;'>
                                <h1 style='color: white; margin: 0; font-size: 24px;'>AutoSys</h1>
                                <p style='color: #bfdbfe; margin: 5px 0 0;'>Sistema de Gestión de Taller Mecánico</p>
                            </div>
                            <div style='background: #ffffff; padding: 30px; border: 1px solid #e5e7eb; border-top: none;'>
                                <h2 style='color: #1f2937; margin-top: 0;'>Restablecimiento de Contraseña</h2>
                                <p style='color: #4b5563;'>Hola <strong>{user.UserName}</strong>,</p>
                                <p style='color: #4b5563;'>Recibimos una solicitud para restablecer la contraseña de su cuenta. Haga clic en el siguiente botón para crear una nueva contraseña:</p>
                                <div style='text-align: center; margin: 30px 0;'>
                                    <a href='{resetLink}' style='background-color: #1e40af; color: white; padding: 14px 32px; text-decoration: none; border-radius: 8px; font-weight: bold; font-size: 16px; display: inline-block;'>Restablecer Contraseña</a>
                                </div>
                                <p style='color: #6b7280; font-size: 14px;'>Si no solicitó este cambio, puede ignorar este correo. Su contraseña no será modificada.</p>
                                <p style='color: #6b7280; font-size: 14px;'>Este enlace expirará en función de la configuración de seguridad del sistema.</p>
                                <hr style='border: none; border-top: 1px solid #e5e7eb; margin: 20px 0;' />
                                <p style='color: #9ca3af; font-size: 12px;'>Si el botón no funciona, copie y pegue este enlace en su navegador:</p>
                                <p style='color: #3b82f6; font-size: 12px; word-break: break-all;'>{resetLink}</p>
                            </div>
                            <div style='background: #f9fafb; padding: 20px; border-radius: 0 0 10px 10px; border: 1px solid #e5e7eb; border-top: none; text-align: center;'>
                                <p style='color: #9ca3af; font-size: 12px; margin: 0;'>© 2025 AutoSys - Sistema de Gestión de Taller</p>
                            </div>
                        </div>";

                    await _emailService.SendEmailAsync(user.Email, "Restablecer contraseña - AutoSys", htmlBody);
                    _logger.LogInformation("Correo de restablecimiento enviado a {Email}", user.Email);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al enviar correo de restablecimiento a {Email}", email);
                    // No revelar error específico al usuario por seguridad
                }
            }

            // Siempre mostrar el mismo mensaje (previene enumeración de emails)
            ViewBag.EmailEnviado = true;
            ViewBag.EmailIngresado = email;
            return View();
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
