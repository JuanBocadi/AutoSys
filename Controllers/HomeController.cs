using System.Diagnostics;
using AutoSys.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;


namespace AutoSys.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;   // Logger registra info(errores, advertencias, mensajes)

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()   
        {
            return View();
        }

        
        public IActionResult Privacy()
        {
            return View();
        }

        
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel  // Crea un modelo de error con el id para identificar errores)
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
