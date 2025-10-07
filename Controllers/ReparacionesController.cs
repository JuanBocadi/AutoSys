using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoSys.Controllers
{
    [Authorize(Roles = "Administrador,Mecanico")]
    public class ReparacionesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}