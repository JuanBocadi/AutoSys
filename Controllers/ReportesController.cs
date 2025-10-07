using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoSys.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class ReportesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}