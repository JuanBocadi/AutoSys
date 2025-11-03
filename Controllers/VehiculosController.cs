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

        // DRILL-DOWN: Ver reparaciones de un vehículo específico
        public async Task<IActionResult> ReparacionesDelVehiculo(int id)
        {
            var vehiculo = await _context.Vehiculos
                .Include(v => v.Cliente)
                .Include(v => v.Ingresos)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (vehiculo == null)
            {
                TempData["ErrorMessage"] = "Vehículo no encontrado.";
                return RedirectToAction("Index");
            }

            return View(vehiculo);
        }
    }
}