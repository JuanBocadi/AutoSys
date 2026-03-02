using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AutoSys.Data;
using AutoSys.Models;
using AutoSys.Filters;
using AutoSys.Services;

namespace AutoSys.Controllers
{
    [Authorize]
    [RequirePermiso("VerFacturacion")]
    public class FacturacionController : Controller
    {
        private readonly AutoSysDbContext _context;
        private readonly IAuditService _auditService;

        public FacturacionController(AutoSysDbContext context, IAuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        public async Task<IActionResult> Index(string buscar = "", string estado = "")
        {
            var query = _context.Facturas
                .Include(f => f.Cliente)
                .Include(f => f.Ingreso)
                    .ThenInclude(i => i!.Vehiculo)
                .AsQueryable();

            if (!string.IsNullOrEmpty(buscar))
            {
                query = query.Where(f => f.NumeroFactura.Contains(buscar) ||
                                        (f.Cliente != null && (f.Cliente.Nombre.Contains(buscar) || 
                                                              f.Cliente.Apellido.Contains(buscar))));
            }

            if (!string.IsNullOrEmpty(estado))
            {
                query = query.Where(f => f.Estado == estado);
            }

            var facturas = await query.OrderByDescending(f => f.FechaEmision).ToListAsync();

            var todasFacturas = await _context.Facturas.ToListAsync();
            ViewBag.TotalFacturas = todasFacturas.Count;
            ViewBag.FacturasPendientes = todasFacturas.Count(f => f.Estado == "Pendiente");
            ViewBag.FacturasPagadas = todasFacturas.Count(f => f.Estado == "Pagada");
            ViewBag.TotalRecaudado = todasFacturas.Where(f => f.Estado == "Pagada").Sum(f => f.Total);
            ViewBag.Buscar = buscar;
            ViewBag.EstadoFiltro = estado;

            return View(facturas);
        }

        [RequirePermiso("CrearFacturas")]
        public async Task<IActionResult> Create(int? ingresoId)
        {
            var ingresosDisponibles = await _context.Ingresos
                .Include(i => i.Vehiculo!)
                    .ThenInclude(v => v.Cliente)
                .Where(i => i.Id == ingresoId || 
                           ((i.Estado == "Finalizado" || i.Estado == "Entregado") &&
                           !_context.Facturas.Any(f => f.IngresoId == i.Id)))
                .ToListAsync();

            ViewBag.Ingresos = new SelectList(
                ingresosDisponibles.Select(i => new
                {
                    i.Id,
                    Texto = $"Ingreso #{i.Id} - {i.Vehiculo!.Patente} - {i.Vehiculo.Cliente!.Nombre} {i.Vehiculo.Cliente.Apellido}"
                }), "Id", "Texto", ingresoId);

            var factura = new Factura();
            
            if (ingresoId.HasValue)
            {
                var ingreso = await _context.Ingresos
                    .Include(i => i.Vehiculo!)
                        .ThenInclude(v => v.Cliente)
                    .Include(i => i.Cliente)
                    .Include(i => i.ServiciosFijos)
                    .FirstOrDefaultAsync(i => i.Id == ingresoId.Value);

                if (ingreso != null)
                {
                    factura.IngresoId = ingreso.Id;
                    factura.ClienteId = ingreso.ClienteId ?? ingreso.Vehiculo!.ClienteId;
                    ViewBag.IngresoSeleccionado = ingreso;
                    
                    if (ingreso.ServiciosFijos != null && ingreso.ServiciosFijos.Any())
                    {
                        ViewBag.ServiciosPredefinidos = ingreso.ServiciosFijos.Select(s => new {
                            Nombre = s.Nombre,
                            Precio = s.PrecioSugerido ?? 0m
                        }).ToList();
                    }
                    else
                    {
                        ViewBag.ServiciosPredefinidos = new List<object> { 
                            new { Nombre = "Servicio: " + (ingreso.Diagnostico ?? "Diagnóstico general"), Precio = 0m } 
                        };
                    }
                }
            }

            var ultimaFactura = await _context.Facturas
                .OrderByDescending(f => f.Id)
                .FirstOrDefaultAsync();
            
            var numeroSecuencial = (ultimaFactura?.Id ?? 0) + 1;
            factura.NumeroFactura = $"F-{DateTime.Now:yyyyMM}-{numeroSecuencial:D4}";

            // Obtener todos los servicios para el autocompletado de las líneas manuales
            ViewBag.TodosServicios = await _context.ServiciosFijos.ToListAsync();

            return View(factura);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermiso("CrearFacturas")]
        public async Task<IActionResult> Create(Factura factura, List<DetalleFactura> detalles)
        {
            // Limpiar cualquier detalle que el ModelBinder haya intentado cargar automáticamente
            // para evitar duplicidad, ya que procesaremos la lista 'detalles' manualmente.
            factura.Detalles.Clear();

            if (detalles == null || !detalles.Any())
            {
                ModelState.AddModelError("", "Debe agregar al menos un detalle a la factura.");
            }

            if (ModelState.IsValid)
            {
                // [BUG FIX CRÍTICO]: Resolvemos el ClienteId desde el IngresoId de forma segura y obligatoria.
                // Sin un ClienteId válido, la base de datos rechazará el INSERT (FK Conflict).
                if (factura.IngresoId > 0)
                {
                    // Buscamos el ingreso con su vehículo de forma explícita
                    var dbIngreso = await _context.Ingresos
                        .Include(i => i.Vehiculo)
                        .FirstOrDefaultAsync(i => i.Id == factura.IngresoId);
                        
                    if (dbIngreso != null && dbIngreso.VehiculoId.HasValue)
                    {
                        // Si el Include falló por alguna razón, recargamos el vehículo
                        var vehiculo = dbIngreso.Vehiculo ?? await _context.Vehiculos.FindAsync(dbIngreso.VehiculoId.Value);
                        if (vehiculo != null)
                        {
                            factura.ClienteId = dbIngreso.ClienteId ?? vehiculo.ClienteId;
                        }
                    }
                }

                if (factura.ClienteId <= 0)
                {
                    ModelState.AddModelError("", "No se pudo identificar al cliente asociado a este ingreso. Por favor, seleccione un ingreso válido.");
                    
                    // Recargar datos para la vista
                    ViewBag.Ingresos = new SelectList(await _context.Ingresos
                        .Include(i => i.Vehiculo!).ThenInclude(v => v.Cliente)
                        .Where(i => (i.Estado == "Finalizado" || i.Estado == "Entregado") &&
                                   !_context.Facturas.Any(f => f.IngresoId == i.Id))
                        .Select(i => new {
                            i.Id,
                            Texto = $"Ingreso #{i.Id} - {i.Vehiculo!.Patente}"
                        }).ToListAsync(), "Id", "Texto", factura.IngresoId);
                        
                    return View(factura);
                }

                factura.Subtotal = 0;

                // ── Validación de stock antes de guardar ──
                for (int i = 0; i < detalles!.Count; i++)
                {
                    var d = detalles[i];

                    // Leer StockId desde el form (puede ser vacío si es servicio manual)
                    var stockIdStr = Request.Form[$"detalles[{i}].StockId"].ToString();
                    if (int.TryParse(stockIdStr, out int stockIdParsed) && stockIdParsed > 0)
                    {
                        d.StockId = stockIdParsed;
                    }
                    else
                    {
                        d.StockId = null;
                    }

                    if (d.StockId.HasValue)
                    {
                        var itemStock = await _context.Stock.FindAsync(d.StockId.Value);
                        if (itemStock == null)
                        {
                            ModelState.AddModelError("", $"El repuesto seleccionado en la línea {i + 1} ya no existe en el inventario.");
                        }
                        // No se valida cantidad de stock: la gestión de inventario es independiente de la facturación.
                    }
                }

                if (!ModelState.IsValid)
                {
                    // Recargar datos necesarios para la vista
                    ViewBag.Ingresos = new SelectList(await _context.Ingresos
                        .Include(i => i.Vehiculo!).ThenInclude(v => v.Cliente)
                        .Where(i => i.Id == factura.IngresoId ||
                                   ((i.Estado == "Finalizado" || i.Estado == "Entregado") &&
                                   !_context.Facturas.Any(f => f.IngresoId == i.Id)))
                        .Select(i => new {
                            i.Id,
                            Texto = $"Ingreso #{i.Id} - {i.Vehiculo!.Patente}"
                        }).ToListAsync(), "Id", "Texto", factura.IngresoId);
                    return View(factura);
                }

                // ── Procesar detalles y descontar stock ──
                for (int i = 0; i < detalles.Count; i++)
                {
                    var d = detalles[i];
                    
                    // Obtenemos el valor crudo del form para evitar la interpretación cultural del ModelBinder
                    // El navegador (type=number) envía siempre un punto como decimal.
                    var precioStr = Request.Form[$"detalles[{i}].PrecioUnitario"].ToString();
                    
                    if (!string.IsNullOrEmpty(precioStr))
                    {
                        // Limpieza extra por si hay ruido, y parseo invariante
                        if (decimal.TryParse(precioStr.Replace(",", "."), 
                            System.Globalization.NumberStyles.Number | System.Globalization.NumberStyles.AllowDecimalPoint, 
                            System.Globalization.CultureInfo.InvariantCulture, out decimal precioLimpio))
                        {
                            d.PrecioUnitario = precioLimpio;
                        }
                    }

                    d.Subtotal = d.Cantidad * d.PrecioUnitario;
                    factura.Subtotal += d.Subtotal;

                    // Stock desconectado de facturación: la gestión de inventario se maneja de forma independiente.
                    
                    // Agregamos el detalle limpio a la colección de la factura
                    factura.Detalles.Add(d);
                }

                factura.IVA = factura.Subtotal * 0.21m; // 21% IVA
                factura.Total = factura.Subtotal + factura.IVA;
                factura.FechaEmision = DateTime.Now;

                // Al agregar la factura, EF agregará automáticamente todos los items en factura.Detalles
                _context.Facturas.Add(factura);
                await _context.SaveChangesAsync();

                // AUDITORÍA
                var rolActual = User.IsInRole("Administrador") ? "Administrador" : User.IsInRole("Recepcionista") ? "Recepcionista" : "Mecanico";
                await _auditService.RegistrarAsync(User.Identity?.Name ?? "desconocido", rolActual, "Facturacion", "Crear",
                    $"Factura {factura.NumeroFactura} creada - Total: ${factura.Total:N2}",
                    factura.Id, factura.NumeroFactura, HttpContext.Connection.RemoteIpAddress?.ToString());

                TempData["SuccessMessage"] = $"Factura {factura.NumeroFactura} creada correctamente.";
                return RedirectToAction(nameof(Detalle), new { id = factura.Id });
            }

            // Recargar datos necesarios para la vista si falla la validación
            ViewBag.Ingresos = new SelectList(await _context.Ingresos
                .Include(i => i.Vehiculo!).ThenInclude(v => v.Cliente)
                .Where(i => i.Id == factura.IngresoId || 
                           ((i.Estado == "Finalizado" || i.Estado == "Entregado") &&
                           !_context.Facturas.Any(f => f.IngresoId == i.Id)))
                .Select(i => new {
                    i.Id,
                    Texto = $"Ingreso #{i.Id} - {i.Vehiculo!.Patente}"
                }).ToListAsync(), "Id", "Texto", factura.IngresoId);

            return View(factura);
        }

        // ── BÚSQUEDA DE REPUESTOS (Autocomplete) ──────────────────────
        [HttpGet]
        public IActionResult BuscarRepuestos(string term)
        {
            if (string.IsNullOrWhiteSpace(term) || term.Length < 2)
                return Json(Array.Empty<object>());

            var termLower = term.Trim().ToLower();

            var resultados = _context.Stock
                .Where(s => s.Nombre.ToLower().Contains(termLower) ||
                            s.Id.ToString().Contains(termLower))
                .OrderBy(s => s.Nombre)
                .Take(10)
                .Select(s => new
                {
                    id = s.Id,
                    label = "#" + s.Id + " - " + s.Nombre,
                    precio = s.PrecioUnitario,
                    stockDisponible = s.Cantidad
                })
                .ToList();

            return Json(resultados);
        }

        public async Task<IActionResult> Detalle(int? id)
        {
            if (id == null) return NotFound();

            var factura = await _context.Facturas
                .Include(f => f.Cliente)
                .Include(f => f.Ingreso)
                    .ThenInclude(i => i!.Vehiculo)
                .Include(f => f.Detalles)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (factura == null) return NotFound();

            return View(factura);
        }

        // POST: Facturacion/CambiarEstado
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermiso("CrearFacturas")]
        public async Task<IActionResult> CambiarEstado(int id, string estado, string? metodoPago)
        {
            var factura = await _context.Facturas.FindAsync(id);
            if (factura == null) return NotFound();

            factura.Estado = estado;
            if (!string.IsNullOrEmpty(metodoPago))
            {
                factura.MetodoPago = metodoPago;
            }

            await _context.SaveChangesAsync();

            // AUDITORÍA
            var rolCambio = User.IsInRole("Administrador") ? "Administrador" : User.IsInRole("Recepcionista") ? "Recepcionista" : "Mecanico";
            await _auditService.RegistrarAsync(User.Identity?.Name ?? "desconocido", rolCambio, "Facturacion", "CambioEstado",
                $"Estado de factura #{factura.Id} cambiado a {estado}",
                factura.Id, factura.NumeroFactura, HttpContext.Connection.RemoteIpAddress?.ToString());

            TempData["SuccessMessage"] = $"Estado de factura actualizado a {estado}.";
            
            return RedirectToAction(nameof(Detalle), new { id });
        }

        public async Task<IActionResult> Imprimir(int? id)
        {
            if (id == null) return NotFound();

            var factura = await _context.Facturas
                .Include(f => f.Cliente)
                .Include(f => f.Ingreso)
                    .ThenInclude(i => i!.Vehiculo)
                .Include(f => f.Detalles)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (factura == null) return NotFound();

            return View(factura);
        }
    }
}