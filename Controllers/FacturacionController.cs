using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AutoSys.Data;
using AutoSys.Models;

namespace AutoSys.Controllers
{
    [Authorize(Roles = "Administrador,Recepcionista")]
    public class FacturacionController : Controller
    {
        private readonly AutoSysDbContext _context;

        public FacturacionController(AutoSysDbContext context)
        {
            _context = context;
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

        public async Task<IActionResult> Create(int? ingresoId)
        {
            var ingresosDisponibles = await _context.Ingresos
                .Include(i => i.Vehiculo!)
                    .ThenInclude(v => v.Cliente)
                .Where(i => (i.Estado == "Finalizado" || i.Estado == "Entregado") &&
                           !_context.Facturas.Any(f => f.IngresoId == i.Id))
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
                    .FirstOrDefaultAsync(i => i.Id == ingresoId.Value);

                if (ingreso != null)
                {
                    factura.IngresoId = ingreso.Id;
                    factura.ClienteId = ingreso.Vehiculo!.ClienteId;
                    ViewBag.IngresoSeleccionado = ingreso;
                }
            }

            var ultimaFactura = await _context.Facturas
                .OrderByDescending(f => f.Id)
                .FirstOrDefaultAsync();
            
            var numeroSecuencial = (ultimaFactura?.Id ?? 0) + 1;
            factura.NumeroFactura = $"F-{DateTime.Now:yyyyMM}-{numeroSecuencial:D4}";

            return View(factura);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Factura factura, List<DetalleFactura> detalles)
        {
            if (ModelState.IsValid && detalles != null && detalles.Any())
            {
                factura.Subtotal = detalles.Sum(d => d.Subtotal);
                factura.IVA = factura.Subtotal * 0.21m; // 21% IVA
                factura.Total = factura.Subtotal + factura.IVA;
                factura.FechaEmision = DateTime.Now;

                _context.Facturas.Add(factura);
                await _context.SaveChangesAsync();

                foreach (var detalle in detalles)
                {
                    detalle.FacturaId = factura.Id;
                    _context.DetallesFactura.Add(detalle);
                }
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Factura {factura.NumeroFactura} creada correctamente.";
                return RedirectToAction(nameof(Detalle), new { id = factura.Id });
            }

            return View(factura);
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