using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoSys.Data;
using AutoSys.Models;
using AutoSys.Filters;
using AutoSys.Services;
using System.Linq;

namespace AutoSys.Controllers
{
    [Authorize]
    [RequirePermiso("VerStock")]
    public class StockController : Controller
    {
        private readonly AutoSysDbContext _context;
        private readonly ILogger<StockController> _logger;
        private readonly IAuditService _auditService;

        public StockController(AutoSysDbContext context, 
                              ILogger<StockController> logger,
                              IAuditService auditService)
        {
            _context = context;
            _logger = logger;
            _auditService = auditService;
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

        [RequirePermiso("CrearStock")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermiso("CrearStock")]
        public async Task<IActionResult> Create(Stock stock)
        {
            if (ModelState.IsValid)
            {
                // Validar nombre duplicado
                var existeNombre = await _context.Stock
                    .AnyAsync(s => s.Nombre == stock.Nombre);
                if (existeNombre)
                {
                    ModelState.AddModelError("Nombre", "Ya existe un item en el inventario con ese nombre.");
                    return View(stock);
                }

                stock.FechaActualizacion = DateTime.Now;
                _context.Stock.Add(stock);
                await _context.SaveChangesAsync();

                // AUDITORÍA
                var rolActual = User.IsInRole("Administrador") ? "Administrador" : User.IsInRole("Recepcionista") ? "Recepcionista" : "Mecanico";
                await _auditService.RegistrarAsync(User.Identity?.Name ?? "desconocido", rolActual, "Stock", "Crear",
                    $"Item creado: {stock.Nombre} (Cantidad: {stock.Cantidad} {stock.Unidad})",
                    stock.Id, stock.Nombre, HttpContext.Connection.RemoteIpAddress?.ToString());

                TempData["SuccessMessage"] = "Item agregado al inventario correctamente.";
                return RedirectToAction(nameof(Index));
            }
            return View(stock);
        }

        [RequirePermiso("EditarStock")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var stock = await _context.Stock.FindAsync(id);
            if (stock == null) return NotFound();

            return View(stock);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermiso("EditarStock")]
        public async Task<IActionResult> Edit(int id, Stock stock)
        {
            if (id != stock.Id) return NotFound();

            if (ModelState.IsValid)
            {
                // Validar nombre duplicado (excluyendo el item actual)
                var existeNombre = await _context.Stock
                    .AnyAsync(s => s.Nombre == stock.Nombre && s.Id != stock.Id);
                if (existeNombre)
                {
                    ModelState.AddModelError("Nombre", "Ya existe otro item en el inventario con ese nombre.");
                    return View(stock);
                }

                try
                {
                    stock.FechaActualizacion = DateTime.Now;
                    _context.Update(stock);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Stock editado: {Id} - {Nombre}", stock.Id, stock.Nombre);

                    // AUDITORÍA
                    var rolEdit = User.IsInRole("Administrador") ? "Administrador" : User.IsInRole("Recepcionista") ? "Recepcionista" : "Mecanico";
                    await _auditService.RegistrarAsync(User.Identity?.Name ?? "desconocido", rolEdit, "Stock", "Editar",
                        $"Item editado: {stock.Nombre}",
                        stock.Id, stock.Nombre, HttpContext.Connection.RemoteIpAddress?.ToString());

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
        [RequirePermiso("AjustarStock")]
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

                // AUDITORÍA
                var rolAjuste = User.IsInRole("Administrador") ? "Administrador" : User.IsInRole("Recepcionista") ? "Recepcionista" : "Mecanico";
                await _auditService.RegistrarAsync(User.Identity?.Name ?? "desconocido", rolAjuste, "Stock", "CambioEstado",
                    $"Ajuste de stock: {stock.Nombre} - {tipo} {cantidad} unidades (Anterior: {cantidadAnterior}, Nuevo: {stock.Cantidad})",
                    stock.Id, stock.Nombre, HttpContext.Connection.RemoteIpAddress?.ToString());

                TempData["SuccessMessage"] = $"Stock ajustado correctamente. Nueva cantidad: {stock.Cantidad} {stock.Unidad}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al ajustar stock. Id: {Id}, Cantidad: {Cantidad}, Tipo: {Tipo}", id, cantidad, tipo);
                TempData["ErrorMessage"] = "Error al ajustar el stock. Por favor, intente nuevamente.";
            }
            
            return RedirectToAction(nameof(Index));
        }

        [RequirePermiso("EliminarStock")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var stock = await _context.Stock.FindAsync(id);
            if (stock == null) return NotFound();

            return View(stock);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [RequirePermiso("EliminarStock")]
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

                    // AUDITORÍA
                    await _auditService.RegistrarAsync(User.Identity?.Name ?? "desconocido", "Administrador", "Stock", "Eliminar",
                        $"Item eliminado: {stock.Nombre}",
                        id, stock.Nombre, HttpContext.Connection.RemoteIpAddress?.ToString());

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