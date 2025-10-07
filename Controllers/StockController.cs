using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoSys.Controllers
{
    [Authorize(Roles = "Administrador,Mecanico")]
    public class StockController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}