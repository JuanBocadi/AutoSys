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
        private readonly ILogger<StockController> _logger;

        public StockController(AutoSysDbContext context, ILogger<StockController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string buscar = "")
        {
            try
            {
                var query = _context.Stock.AsQueryable();

                if (!string.IsNullOrEmpty(buscar))
                {
                    query = query.Where(s => s.Nombre.Contains(buscar) || 
                                            (s.Descripcion != null && s.Descripcion.Contains(buscar)));
                }

                var items = await query.OrderBy(s => s.Nombre).ToListAsync();

                ViewBag.TotalItems = items.Count;
                ViewBag.ItemsSuficientes = items.Count(s => s.ColorSemaforo == "success");
                ViewBag.ItemsBajo = items.Count(s => s.ColorSemaforo == "warning");
                ViewBag.ItemsCriticos = items.Count(s => s.ColorSemaforo == "danger");
                ViewBag.Buscar = buscar;

                return View(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar el índice de stock");
                TempData["ErrorMessage"] = "Error al cargar los datos de stock.";
                return View(new List<Stock>());
            }
        }

        [Authorize(Roles = "Administrador")]
        public IActionResult Create()
        {
            return View();
        }

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

        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var stock = await _context.Stock.FindAsync(id);
            if (stock == null) return NotFound();

            return View(stock);
        }

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
                    _logger.LogInformation("Stock editado: {Id} - {Nombre}", stock.Id, stock.Nombre);
                    TempData["SuccessMessage"] = "Item actualizado correctamente.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!StockExists(stock.Id))
                    {
                        _logger.LogWarning("Item no encontrado al editar: {Id}", id);
                        return NotFound();
                    }
                    else
                    {
                        _logger.LogError(ex, "Error de concurrencia al editar stock {Id}", id);
                        TempData["ErrorMessage"] = "Error al actualizar. El item pudo haber sido modificado por otro usuario.";
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al editar stock {Id}", id);
                    TempData["ErrorMessage"] = "Error al actualizar el item.";
                }
            }
            return View(stock);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AjustarStock(int id, int cantidad, string tipo)
        {
            try
            {
                _logger.LogInformation("Iniciando ajuste de stock. Id: {Id}, Cantidad: {Cantidad}, Tipo: {Tipo}", id, cantidad, tipo);
                
                var stock = await _context.Stock.FindAsync(id);
                if (stock == null)
                {
                    _logger.LogWarning("Item no encontrado para ajustar: {Id}", id);
                    TempData["ErrorMessage"] = "El item no fue encontrado.";
                    return RedirectToAction(nameof(Index));
                }

                if (cantidad <= 0)
                {
                    _logger.LogWarning("Cantidad inválida para ajuste: {Cantidad}", cantidad);
                    TempData["ErrorMessage"] = "La cantidad debe ser mayor a cero.";
                    return RedirectToAction(nameof(Index));
                }

                var cantidadAnterior = stock.Cantidad;

                if (tipo == "agregar")
                {
                    stock.Cantidad += cantidad;
                    _logger.LogInformation("Agregando {Cantidad} unidades al stock {Id}. Anterior: {Anterior}, Nuevo: {Nuevo}", 
                        cantidad, id, cantidadAnterior, stock.Cantidad);
                }
                else if (tipo == "quitar")
                {
                    if (stock.Cantidad - cantidad < 0)
                    {
                        _logger.LogWarning("Stock insuficiente. Disponible: {Disponible}, Solicitado: {Solicitado}", 
                            stock.Cantidad, cantidad);
                        TempData["ErrorMessage"] = "No hay suficiente stock disponible.";
                        return RedirectToAction(nameof(Index));
                    }
                    stock.Cantidad -= cantidad;
                    _logger.LogInformation("Quitando {Cantidad} unidades del stock {Id}. Anterior: {Anterior}, Nuevo: {Nuevo}", 
                        cantidad, id, cantidadAnterior, stock.Cantidad);
                }
                else
                {
                    _logger.LogWarning("Tipo de ajuste inválido: {Tipo}", tipo);
                    TempData["ErrorMessage"] = "Tipo de ajuste inválido.";
                    return RedirectToAction(nameof(Index));
                }

                stock.FechaActualizacion = DateTime.Now;
                _context.Update(stock);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Stock ajustado exitosamente. Item: {Nombre}, Nueva cantidad: {Cantidad}", 
                    stock.Nombre, stock.Cantidad);
                TempData["SuccessMessage"] = $"Stock ajustado correctamente. Nueva cantidad: {stock.Cantidad} {stock.Unidad}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al ajustar stock. Id: {Id}, Cantidad: {Cantidad}, Tipo: {Tipo}", id, cantidad, tipo);
                TempData["ErrorMessage"] = "Error al ajustar el stock. Por favor, intente nuevamente.";
            }
            
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var stock = await _context.Stock.FindAsync(id);
            if (stock == null) return NotFound();

            return View(stock);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                _logger.LogInformation("Iniciando eliminación de stock. Id: {Id}", id);
                
                var stock = await _context.Stock.FindAsync(id);
                if (stock != null)
                {
                    _context.Stock.Remove(stock);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Stock eliminado exitosamente. Id: {Id}, Nombre: {Nombre}", id, stock.Nombre);
                    TempData["SuccessMessage"] = "Item eliminado del inventario correctamente.";
                }
                else
                {
                    _logger.LogWarning("Item no encontrado para eliminar. Id: {Id}", id);
                    TempData["ErrorMessage"] = "El item no fue encontrado.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar stock. Id: {Id}", id);
                TempData["ErrorMessage"] = "Error al eliminar el item. Por favor, intente nuevamente.";
            }
            
            return RedirectToAction(nameof(Index));
        }

        private bool StockExists(int id)
        {
            return _context.Stock.Any(e => e.Id == id);
        }
    }
}