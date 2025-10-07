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
    }
}