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

        // UserManager para poder consultar los usuarios
        public UsuariosController(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        // Esta acción prepara y muestra la lista de usuarios
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
                    Email = user.Email ?? string.Empty,
                    Rol = roles.FirstOrDefault() ?? "Sin rol" // Mostramos el primer rol, o texto por defecto
                });
            }

            return View(userViewModels);
        }

        // devuelve los datos de un usuario en formato JSON para el popup
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
                Email = user.Email ?? string.Empty,
                Rol = roles.FirstOrDefault() ?? "Sin rol"
            };

            return Json(userDetails);
        }

        // Eliminar usuario (solo Administrador)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest();
            }

            // Evitar que el usuario autenticado elimine su propia cuenta
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
    }
}