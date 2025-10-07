using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoSys.Controllers
{
    [Authorize(Roles = "Administrador,Recepcionista")]
    public class FacturacionController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}