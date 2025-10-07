using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoSys.Data;
using AutoSys.Models;
using System.Linq;

namespace AutoSys.Controllers
{
    [Authorize(Roles = "Administrador,Recepcionista")]
    public class VehiculosController : Controller
    {
        private readonly AutoSysDbContext _context;

        public VehiculosController(AutoSysDbContext context)
        {
            _context = context;
        }

        // Listado solo lectura de vehículos
        public async Task<IActionResult> Index()
        {
            var vehiculos = await _context.Vehiculos
                .Include(v => v.Cliente)
                .OrderBy(v => v.Patente)
                .ToListAsync();
            return View(vehiculos);
        }
    }
}