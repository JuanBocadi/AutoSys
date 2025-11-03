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
        public async Task<IActionResult> Index(string buscar = "")
        {
            var query = _context.Stock.AsQueryable();

            if (!string.IsNullOrEmpty(buscar))
            {
                query = query.Where(s => s.Nombre.Contains(buscar) || 
                                        (s.Descripcion != null && s.Descripcion.Contains(buscar)));
            }

            var items = await query.OrderBy(s => s.Nombre).ToListAsync();

            // Estadísticas para el dashboard
            ViewBag.TotalItems = items.Count;
            ViewBag.ItemsSuficientes = items.Count(s => s.ColorSemaforo == "success");
            ViewBag.ItemsBajo = items.Count(s => s.ColorSemaforo == "warning");
            ViewBag.ItemsCriticos = items.Count(s => s.ColorSemaforo == "danger");
            ViewBag.Buscar = buscar;

            return View(items);
        }

        // GET: Stock/Create
        [Authorize(Roles = "Administrador")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Stock/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Create(Stock stock)
        {
            if (ModelState.IsValid)
            {
                stock.FechaActualizacion = DateTime.Now;
                _context.Stock.Add(stock);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Item agregado al inventario correctamente.";
                return RedirectToAction(nameof(Index));
            }
            return View(stock);
        }

        // GET: Stock/Edit/5
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var stock = await _context.Stock.FindAsync(id);
            if (stock == null) return NotFound();

            return View(stock);
        }

        // POST: Stock/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Edit(int id, Stock stock)
        {
            if (id != stock.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    stock.FechaActualizacion = DateTime.Now;
                    _context.Update(stock);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Item actualizado correctamente.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StockExists(stock.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(stock);
        }

        // POST: Stock/AjustarStock
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AjustarStock(int id, int cantidad, string tipo)
        {
            var stock = await _context.Stock.FindAsync(id);
            if (stock == null) return NotFound();

            if (tipo == "agregar")
            {
                stock.Cantidad += cantidad;
            }
            else if (tipo == "quitar")
            {
                if (stock.Cantidad - cantidad < 0)
                {
                    TempData["ErrorMessage"] = "No hay suficiente stock disponible.";
                    return RedirectToAction(nameof(Index));
                }
                stock.Cantidad -= cantidad;
            }

            stock.FechaActualizacion = DateTime.Now;
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Stock ajustado correctamente. Nueva cantidad: {stock.Cantidad}";
            return RedirectToAction(nameof(Index));
        }

        // GET: Stock/Delete/5
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var stock = await _context.Stock.FindAsync(id);
            if (stock == null) return NotFound();

            return View(stock);
        }

        // POST: Stock/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var stock = await _context.Stock.FindAsync(id);
            if (stock != null)
            {
                _context.Stock.Remove(stock);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Item eliminado del inventario.";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool StockExists(int id)
        {
            return _context.Stock.Any(e => e.Id == id);
        }
    }
}