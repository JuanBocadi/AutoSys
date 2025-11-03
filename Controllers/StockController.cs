using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoSys.Data;
using AutoSys.Models;
using System.Linq;

namespace AutoSys.Controllers
{
    [Authorize(Roles = "Administrador,Mecanico")]
    public class StockController : Controller
    {
        private readonly AutoSysDbContext _context;

        public StockController(AutoSysDbContext context)
        {
            _context = context;
        }

        // Vista principal de stock con semaforización
        public async Task<IActionResult> Index()
        {
            var items = await _context.Stock
                .OrderBy(s => s.Nombre)
                .ToListAsync();

            // Estadísticas para el dashboard
            ViewBag.TotalItems = items.Count;
            ViewBag.ItemsSuficientes = items.Count(s => s.ColorSemaforo == "success");
            ViewBag.ItemsBajo = items.Count(s => s.ColorSemaforo == "warning");
            ViewBag.ItemsCriticos = items.Count(s => s.ColorSemaforo == "danger");

            return View(items);
        }
    }
}