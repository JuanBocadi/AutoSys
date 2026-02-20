using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoSys.Data;
using AutoSys.Models;
using AutoSys.Filters;
using System.Linq;

namespace AutoSys.Controllers
{
    [Authorize(Roles = "Administrador,Recepcionista,Mecanico")]
    [RequirePermiso("VerReparaciones")]
    public class ReparacionesController : Controller
    {
        private readonly AutoSysDbContext _context;

        public ReparacionesController(AutoSysDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var reparaciones = await _context.Ingresos
                .Include(i => i.Vehiculo!)
                    .ThenInclude(v => v.Cliente)
                .OrderByDescending(i => i.FechaIngreso)
                .ToListAsync();

            ViewBag.TotalReparaciones = reparaciones.Count;
            ViewBag.ReparacionesActivas = reparaciones.Count(r => !r.FechaEgreso.HasValue);

            return View(reparaciones);
        }
    }
}