using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AutoSys.Services
{
    /// Registro de metadatos de un backup realizado, persistido en JSON
    public class BackupRecord
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string FileName { get; set; } = string.Empty;
        public string? FilesZipName { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public long SizeBytes { get; set; }
        public string Type { get; set; } = "Completo";       // Completo | Diferencial
        public string Status { get; set; } = "Exitoso";      // Exitoso | Fallido | Verificado
        public string Origin { get; set; } = "Manual";       // Manual | Automático
        public bool IncludesFiles { get; set; }
        public string Notes { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
    }

    /// Contrato para el servicio de backup y restauración de la BD con SQL Server
    public interface IBackupService
    {
        /// Crea un backup completo de la BD y opcionalmente de los archivos subidos
        Task<BackupRecord> CreateBackupAsync(string createdBy, string? notes = null, bool includeFiles = true);

        /// Crea un backup diferencial (solo cambios desde el último completo)
        Task<BackupRecord> CreateDifferentialBackupAsync(string createdBy, string? notes = null);

        /// Verifica la integridad de un backup con RESTORE VERIFYONLY
        Task<bool> VerifyBackupAsync(string backupId);

        /// Restaura la BD desde un backup (operación destructiva)
        Task<bool> RestoreBackupAsync(string backupId);

        /// Elimina un archivo de backup del disco y del catálogo
        Task<bool> DeleteBackupAsync(string backupId);

        /// Obtiene todo el historial de backups
        List<BackupRecord> GetBackupHistory();

        /// Obtiene un registro de backup por su Id
        BackupRecord? GetBackup(string backupId);

        /// Devuelve la ruta completa del archivo .bak de un backup
        string GetBackupFilePath(string backupId);

        /// Devuelve el directorio donde se almacenan los backups
        string GetBackupDirectory();
    }
}
