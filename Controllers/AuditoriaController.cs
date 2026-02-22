using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutoSys.Services;

namespace AutoSys.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class AuditoriaController : Controller
    {
        private readonly IAuditService _auditService;

        public AuditoriaController(IAuditService auditService)
        {
            _auditService = auditService;
        }

        public async Task<IActionResult> Index(DateTime? desde, DateTime? hasta, string? categoria, string? usuario)
        {
            // Defaults: última semana
            desde ??= DateTime.Today.AddDays(-7);
            hasta ??= DateTime.Today;

            var logs = await _auditService.ObtenerLogsAsync(desde, hasta, categoria, usuario);
            var categorias = await _auditService.ObtenerCategoriasAsync();

            ViewBag.Desde = desde.Value.ToString("yyyy-MM-dd");
            ViewBag.Hasta = hasta.Value.ToString("yyyy-MM-dd");
            ViewBag.CategoriaSeleccionada = categoria;
            ViewBag.UsuarioFiltro = usuario;
            ViewBag.Categorias = categorias;
            ViewBag.TotalRegistros = logs.Count;

            return View(logs);
        }
    }
}
