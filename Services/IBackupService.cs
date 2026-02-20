using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AutoSys.Services
{
    /// <summary>
    /// Registro de metadatos de un backup realizado.
    /// Se persiste en archivo JSON (no en la BD, ya que un restore
    /// sobrescribiría la información).
    /// </summary>
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

    /// <summary>
    /// Contrato para el servicio de resguardo y restauración de la base de datos.
    /// Utiliza los comandos nativos BACKUP DATABASE / RESTORE DATABASE de SQL Server.
    /// </summary>
    public interface IBackupService
    {
        /// <summary>Crea un backup completo de la base de datos (y opcionalmente de los archivos subidos).</summary>
        Task<BackupRecord> CreateBackupAsync(string createdBy, string? notes = null, bool includeFiles = true);

        /// <summary>Crea un backup diferencial de la base de datos (solo cambios desde el último completo).</summary>
        Task<BackupRecord> CreateDifferentialBackupAsync(string createdBy, string? notes = null);

        /// <summary>Verifica la integridad de un backup existente con RESTORE VERIFYONLY.</summary>
        Task<bool> VerifyBackupAsync(string backupId);

        /// <summary>Restaura la base de datos desde un backup. OPERACIÓN DESTRUCTIVA.</summary>
        Task<bool> RestoreBackupAsync(string backupId);

        /// <summary>Elimina un archivo de backup del disco y del catálogo.</summary>
        Task<bool> DeleteBackupAsync(string backupId);

        /// <summary>Obtiene todo el historial de backups.</summary>
        List<BackupRecord> GetBackupHistory();

        /// <summary>Obtiene un registro de backup por su Id.</summary>
        BackupRecord? GetBackup(string backupId);

        /// <summary>Devuelve la ruta completa del archivo .bak de un backup.</summary>
        string GetBackupFilePath(string backupId);

        /// <summary>Devuelve el directorio donde se almacenan los backups.</summary>
        string GetBackupDirectory();
    }
}
