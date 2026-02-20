using AutoSys.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoSys.Controllers
{
    /// <summary>
    /// Controlador de Resguardo y Restauración.
    /// Accesible únicamente por usuarios con rol Administrador.
    /// Permite crear, verificar, descargar, eliminar y restaurar backups
    /// de la base de datos y archivos del sistema.
    /// </summary>
    [Authorize(Roles = "Administrador")]
    public class BackupController : Controller
    {
        private readonly IBackupService _backupService;
        private readonly BackupSchedulerService _scheduler;
        private readonly ILogger<BackupController> _logger;

        public BackupController(
            IBackupService backupService,
            BackupSchedulerService scheduler,
            ILogger<BackupController> logger)
        {
            _backupService = backupService;
            _scheduler = scheduler;
            _logger = logger;
        }

        // ───────────────────── LISTADO DE BACKUPS ─────────────────────

        public IActionResult Index()
        {
            var backups = _backupService.GetBackupHistory();

            ViewBag.TotalBackups = backups.Count;
            ViewBag.UltimoBackup = backups.FirstOrDefault()?.CreatedAt.ToString("dd/MM/yyyy HH:mm") ?? "Nunca";
            ViewBag.TamanoTotal = backups.Sum(b => b.SizeBytes);
            ViewBag.BackupsExitosos = backups.Count(b => b.Status == "Exitoso" || b.Status == "Verificado");
            ViewBag.BackupDirectory = _backupService.GetBackupDirectory();

            // Info del scheduler
            var scheduleState = _scheduler.LoadState();
            ViewBag.ScheduleEnabled = scheduleState.Enabled;
            ViewBag.NextFullBackup = scheduleState.NextFullBackup?.ToString("dd/MM/yyyy HH:mm") ?? "No programado";
            ViewBag.NextDiffBackup = scheduleState.NextDiffBackup?.ToString("dd/MM/yyyy HH:mm") ?? "No programado";
            ViewBag.LastAutoFull = scheduleState.LastAutoFullBackup?.ToString("dd/MM/yyyy HH:mm") ?? "Nunca";
            ViewBag.LastAutoDiff = scheduleState.LastAutoDiffBackup?.ToString("dd/MM/yyyy HH:mm") ?? "Nunca";
            ViewBag.FullIntervalHours = _scheduler.FullIntervalHours;
            ViewBag.DiffIntervalHours = _scheduler.DiffIntervalHours;

            return View(backups);
        }

        // ───────────────────── CREAR BACKUP ─────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string? notes, bool includeFiles = true, string type = "Completo")
        {
            var username = User.Identity?.Name ?? "admin";

            _logger.LogInformation("Usuario {Username} solicitó creación de backup {Type}", username, type);

            BackupRecord record;
            if (type == "Diferencial")
            {
                record = await _backupService.CreateDifferentialBackupAsync(username, notes);
            }
            else
            {
                record = await _backupService.CreateBackupAsync(username, notes, includeFiles);
            }

            if (record.Status == "Exitoso")
            {
                TempData["SuccessMessage"] = $"Backup {record.Type} creado exitosamente: {record.FileName} ({FormatBytes(record.SizeBytes)})";
            }
            else
            {
                TempData["ErrorMessage"] = $"Error al crear el backup. {record.Notes}";
            }

            return RedirectToAction(nameof(Index));
        }

        // ───────────────────── TOGGLE SCHEDULER ─────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleScheduler(bool enable)
        {
            _scheduler.SetEnabled(enable);
            TempData["SuccessMessage"] = enable
                ? "Backups automáticos HABILITADOS. Se ejecutarán según el cronograma configurado."
                : "Backups automáticos DESHABILITADOS. Solo se realizarán backups manuales.";
            return RedirectToAction(nameof(Index));
        }

        // ───────────────────── VERIFICAR BACKUP ─────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Verify(string id)
        {
            var result = await _backupService.VerifyBackupAsync(id);

            if (result)
            {
                TempData["SuccessMessage"] = "El backup fue verificado correctamente. La integridad de los datos es válida.";
            }
            else
            {
                TempData["ErrorMessage"] = "La verificación del backup falló. El archivo puede estar corrupto o incompleto.";
            }

            return RedirectToAction(nameof(Index));
        }

        // ───────────────────── DESCARGAR BACKUP ─────────────────────

        public IActionResult Download(string id)
        {
            var record = _backupService.GetBackup(id);
            if (record == null)
            {
                TempData["ErrorMessage"] = "Backup no encontrado.";
                return RedirectToAction(nameof(Index));
            }

            var filePath = _backupService.GetBackupFilePath(id);
            if (!System.IO.File.Exists(filePath))
            {
                TempData["ErrorMessage"] = "El archivo de backup no existe en el disco.";
                return RedirectToAction(nameof(Index));
            }

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/octet-stream", record.FileName);
        }

        // ───────────────────── ELIMINAR BACKUP ─────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var record = _backupService.GetBackup(id);
            var result = await _backupService.DeleteBackupAsync(id);

            if (result)
            {
                TempData["SuccessMessage"] = $"Backup eliminado: {record?.FileName}";
            }
            else
            {
                TempData["ErrorMessage"] = "No se pudo eliminar el backup.";
            }

            return RedirectToAction(nameof(Index));
        }

        // ───────────────────── RESTAURAR BACKUP ─────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(string id)
        {
            var record = _backupService.GetBackup(id);
            if (record == null)
            {
                TempData["ErrorMessage"] = "Backup no encontrado.";
                return RedirectToAction(nameof(Index));
            }

            _logger.LogWarning("Usuario {Username} solicitó restauración desde backup {FileName}",
                User.Identity?.Name, record.FileName);

            var result = await _backupService.RestoreBackupAsync(id);

            if (result)
            {
                TempData["SuccessMessage"] = $"Base de datos restaurada exitosamente desde: {record.FileName}. Se recomienda reiniciar la aplicación.";
            }
            else
            {
                TempData["ErrorMessage"] = "Error al restaurar la base de datos. Verifique los logs del sistema.";
            }

            return RedirectToAction(nameof(Index));
        }

        // ───────────────────── INSTRUCTIVO ─────────────────────

        public IActionResult Instructivo()
        {
            return View();
        }

        // ───────────────────── UTILIDADES ─────────────────────

        private static string FormatBytes(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            int order = 0;
            double size = bytes;
            while (size >= 1024 && order < sizes.Length - 1)
            {
                order++;
                size /= 1024;
            }
            return $"{size:0.##} {sizes[order]}";
        }
    }
}
