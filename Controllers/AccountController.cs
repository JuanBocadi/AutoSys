using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AutoSys.ViewModels;
using AutoSys.Patterns.Composite;
using AutoSys.Patterns.Singleton;
using System.Threading.Tasks;
using System.Linq;

namespace AutoSys.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(UserManager<IdentityUser> userManager,
                                 SignInManager<IdentityUser> signInManager,
                                 RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
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
                var result = await _signInManager.PasswordSignInAsync(user.UserName!, password, false, false);
                
                if (result.Succeeded)
                    return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Usuario o contraseña incorrectos.";
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
