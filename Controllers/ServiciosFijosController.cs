using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoSys.Data;
using AutoSys.Models;
using AutoSys.Filters;
using Microsoft.AspNetCore.Authorization;

namespace AutoSys.Controllers
{
    [Authorize]
    [RequirePermiso("VerServiciosFijos")]
    public class ServiciosFijosController : Controller
    {
        private readonly AutoSysDbContext _context;

        public ServiciosFijosController(AutoSysDbContext context)
        {
            _context = context;
        }

        // GET: ServiciosFijos
        public async Task<IActionResult> Index()
        {
            return View(await _context.ServiciosFijos.ToListAsync());
        }

        // GET: ServiciosFijos/Create
        [RequirePermiso("GestionarServiciosFijos")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: ServiciosFijos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermiso("GestionarServiciosFijos")]
        public async Task<IActionResult> Create([Bind("Id,Nombre,DescripcionPredeterminada,PrecioSugerido")] ServicioFijo servicioFijo)
        {
            if (ModelState.IsValid)
            {
                _context.Add(servicioFijo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(servicioFijo);
        }

        // GET: ServiciosFijos/Edit/5
        [RequirePermiso("GestionarServiciosFijos")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var servicioFijo = await _context.ServiciosFijos.FindAsync(id);
            if (servicioFijo == null) return NotFound();
            
            return View(servicioFijo);
        }

        // POST: ServiciosFijos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermiso("GestionarServiciosFijos")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,DescripcionPredeterminada,PrecioSugerido")] ServicioFijo servicioFijo)
        {
            if (id != servicioFijo.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(servicioFijo);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ServicioFijoExists(servicioFijo.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(servicioFijo);
        }

        // GET: ServiciosFijos/Delete/5
        [RequirePermiso("GestionarServiciosFijos")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var servicioFijo = await _context.ServiciosFijos
                .FirstOrDefaultAsync(m => m.Id == id);
            if (servicioFijo == null) return NotFound();

            return View(servicioFijo);
        }

        // POST: ServiciosFijos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [RequirePermiso("GestionarServiciosFijos")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var servicioFijo = await _context.ServiciosFijos.FindAsync(id);
            if (servicioFijo != null)
            {
                _context.ServiciosFijos.Remove(servicioFijo);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ServicioFijoExists(int id)
        {
            return _context.ServiciosFijos.Any(e => e.Id == id);
        }
    }
}
