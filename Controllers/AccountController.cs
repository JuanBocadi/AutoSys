using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AutoSys.ViewModels;
using AutoSys.Patterns.Composite;
using AutoSys.Patterns.Strategy;
using AutoSys.Patterns.Builder;
using System.Threading.Tasks;
using System.Linq;

namespace AutoSys.Controllers
{
    /// <summary>
    /// Controlador de cuenta que implementa los patrones de diseño:
    /// - Composite: Para validación de Login y Register
    /// - Strategy: Para estrategias de autenticación
    /// - Builder: Para construcción de usuarios
    /// </summary>
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        
        // Patrón Composite - Validadores
        private readonly ValidationComposite _loginValidator;
        private readonly ValidationComposite _registerValidator;
        
        // Patrón Strategy - Contexto de autenticación
        private readonly AuthenticationContext _authContext;
        
        // Patrón Builder - Director para crear usuarios
        private readonly UserDirector _userDirector;

        public AccountController(UserManager<IdentityUser> userManager,
                                 SignInManager<IdentityUser> signInManager,
                                 RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            
            // Inicializar el patrón Composite para validaciones
            _loginValidator = ValidationFactory.CreateLoginValidator();
            _registerValidator = ValidationFactory.CreateRegisterValidator();
            
            // Inicializar el patrón Strategy para autenticación
            var strategies = new List<IAuthenticationStrategy>
            {
                new UsernameAuthenticationStrategy(userManager),
                new EmailAuthenticationStrategy(userManager)
            };
            _authContext = new AuthenticationContext(signInManager, strategies);
            
            // Inicializar el patrón Builder para creación de usuarios
            var userBuilder = new IdentityUserBuilder(userManager, roleManager);
            _userDirector = new UserDirector(userBuilder);
        }

        [HttpGet]
        [Authorize(Roles = "Administrador")]
        public IActionResult Register()
        {
            ViewBag.Roles = _roleManager.Roles.Select(r => r.Name).ToList();
            return View();
        }

        /// <summary>
        /// Procesa el registro de usuarios utilizando:
        /// - Patrón Composite: Para validación de datos
        /// - Patrón Builder: Para construcción del usuario
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            // Aplicar validación con patrón Composite
            var validationContext = new Dictionary<string, object?>
            {
                { "Username", model.Username },
                { "Email", model.Email },
                { "Password", model.Password },
                { "ConfirmPassword", model.ConfirmPassword },
                { "Rol", model.Rol }
            };

            var validationResult = _registerValidator.Validate(validationContext);
            
            if (!validationResult.IsValid)
            {
                foreach (var error in validationResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
                ViewBag.Roles = _roleManager.Roles.Select(r => r.Name).ToList();
                return View(model);
            }

            // Usar el patrón Builder para crear el usuario
            var buildResult = await _userDirector.BuildStandardUserAsync(
                model.Username,
                model.Email,
                model.Password,
                model.Rol);

            if (buildResult.Succeeded)
            {
                TempData["Success"] = "Usuario registrado correctamente.";
                return RedirectToAction("Index", "Usuarios");
            }

            foreach (var error in buildResult.Errors)
                ModelState.AddModelError(string.Empty, error);

            ViewBag.Roles = _roleManager.Roles.Select(r => r.Name).ToList();
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        /// <summary>
        /// Procesa el inicio de sesión utilizando:
        /// - Patrón Composite: Para validación de datos
        /// - Patrón Strategy: Para probar diferentes estrategias de autenticación
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string email, string password)
        {
            // Aplicar validación con patrón Composite
            var validationContext = new Dictionary<string, object?>
            {
                { "Email", email },
                { "Password", password }
            };

            var validationResult = _loginValidator.Validate(validationContext);
            
            if (!validationResult.IsValid)
            {
                ViewBag.Error = string.Join(" ", validationResult.Errors);
                return View();
            }

            // Usar el patrón Strategy para autenticación
            var authResult = await _authContext.AuthenticateAsync(email, password);
            
            if (authResult.IsSuccess)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = authResult.ErrorMessage ?? "Usuario o contraseña incorrectos.";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }
    }
}
