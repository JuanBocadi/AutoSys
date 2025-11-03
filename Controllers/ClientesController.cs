using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoSys.Data;
using AutoSys.Models;
using System.Linq;

namespace AutoSys.Controllers
{
    [Authorize(Roles = "Administrador,Recepcionista")]
    public class ClientesController : Controller
    {
        private readonly AutoSysDbContext _context;

        public ClientesController(AutoSysDbContext context)
        {
            _context = context;
        }

        // Listado solo lectura de clientes
        public async Task<IActionResult> Index()
        {
            var clientes = await _context.Clientes
                .Include(c => c.Vehiculos)
                .OrderBy(c => c.Apellido)
                .ThenBy(c => c.Nombre)
                .ToListAsync();
            return View(clientes);
        }

        // DRILL-DOWN: Ver vehículos de un cliente específico
        public async Task<IActionResult> VehiculosDelCliente(int id)
        {
            var cliente = await _context.Clientes
                .Include(c => c.Vehiculos)
                    .ThenInclude(v => v.Ingresos)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cliente == null)
            {
                TempData["ErrorMessage"] = "Cliente no encontrado.";
                return RedirectToAction("Index");
            }

            return View(cliente);
        }
    }
}