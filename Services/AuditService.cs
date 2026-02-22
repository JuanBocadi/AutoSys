using AutoSys.Data;
using AutoSys.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoSys.Services
{
    /// <summary>
    /// Implementación del servicio de auditoría.
    /// Persiste registros de acciones importantes en la base de datos.
    /// </summary>
    public class AuditService : IAuditService
    {
        private readonly AutoSysDbContext _context;
        private readonly ILogger<AuditService> _logger;

        public AuditService(AutoSysDbContext context, ILogger<AuditService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task RegistrarAsync(string usuario, string rol, string categoria, string accion,
                                          string descripcion, int? entidadId = null, string? entidadNombre = null,
                                          string? direccionIP = null)
        {
            try
            {
                var log = new AuditLog
                {
                    Fecha = DateTime.Now,
                    Usuario = usuario,
                    Rol = rol,
                    Categoria = categoria,
                    Accion = accion,
                    Descripcion = descripcion,
                    EntidadId = entidadId,
                    EntidadNombre = entidadNombre,
                    DireccionIP = direccionIP
                };

                _context.AuditLogs.Add(log);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Auditoría: [{Categoria}] {Accion} por {Usuario} - {Descripcion}",
                    categoria, accion, usuario, descripcion);
            }
            catch (Exception ex)
            {
                // La auditoría nunca debe bloquear la operación principal
                _logger.LogError(ex, "Error al registrar auditoría: {Descripcion}", descripcion);
            }
        }

        public async Task<List<AuditLog>> ObtenerLogsAsync(DateTime? desde = null, DateTime? hasta = null,
                                                            string? categoria = null, string? usuario = null,
                                                            int cantidad = 200)
        {
            var query = _context.AuditLogs.AsQueryable();

            if (desde.HasValue)
                query = query.Where(l => l.Fecha >= desde.Value);
            if (hasta.HasValue)
                query = query.Where(l => l.Fecha <= hasta.Value.Date.AddDays(1));
            if (!string.IsNullOrEmpty(categoria))
                query = query.Where(l => l.Categoria == categoria);
            if (!string.IsNullOrEmpty(usuario))
                query = query.Where(l => l.Usuario.Contains(usuario));

            return await query
                .OrderByDescending(l => l.Fecha)
                .Take(cantidad)
                .ToListAsync();
        }

        public async Task<List<string>> ObtenerCategoriasAsync()
        {
            return await _context.AuditLogs
                .Select(l => l.Categoria)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
        }
    }
}
