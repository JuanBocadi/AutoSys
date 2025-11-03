using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoSys.Data;
using AutoSys.Models;
using System.Linq;

namespace AutoSys.Controllers
{
    [Authorize(Roles = "Administrador,Mecanico")]
    public class ReparacionesController : Controller
    {
        private readonly AutoSysDbContext _context;

        public ReparacionesController(AutoSysDbContext context)
        {
            _context = context;
        }

        // Vista principal de reparaciones con semaforización
        public async Task<IActionResult> Index()
        {
            var reparaciones = await _context.Ingresos
                .Include(i => i.Vehiculo!)
                    .ThenInclude(v => v.Cliente)
                .OrderByDescending(i => i.FechaIngreso)
                .ToListAsync();

            // Estadísticas para el dashboard
            ViewBag.TotalReparaciones = reparaciones.Count;
            // En Taller: Todos los que NO tienen fecha de egreso (incluye todos los estados sin entrega)
            ViewBag.ReparacionesActivas = reparaciones.Count(r => !r.FechaEgreso.HasValue);

            return View(reparaciones);
        }
    }
}