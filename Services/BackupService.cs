using Microsoft.Data.SqlClient;
using System.IO.Compression;
using System.Text.Json;

namespace AutoSys.Services
{
    /// <summary>
    /// Implementación del servicio de resguardo y restauración.
    /// Utiliza los comandos T-SQL nativos de SQL Server para las operaciones
    /// de BACKUP / RESTORE, y administra un catálogo JSON local con el
    /// historial de backups (independiente de la BD para sobrevivir a un restore).
    /// </summary>
    public class BackupService : IBackupService
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<BackupService> _logger;
        private readonly string _backupDirectory;
        private readonly string _catalogPath;

        public BackupService(
            IConfiguration configuration,
            IWebHostEnvironment env,
            ILogger<BackupService> logger)
        {
            _configuration = configuration;
            _env = env;
            _logger = logger;

            // Directorio configurable; por defecto: {ContentRoot}/Backups
            var configDir = configuration["Backup:Directory"];
            _backupDirectory = string.IsNullOrWhiteSpace(configDir)
                ? Path.Combine(env.ContentRootPath, "Backups")
                : configDir;

            if (!Directory.Exists(_backupDirectory))
                Directory.CreateDirectory(_backupDirectory);

            _catalogPath = Path.Combine(_backupDirectory, "backup_catalog.json");
        }

        // ───────────────────────── CREAR BACKUP ─────────────────────────

        public async Task<BackupRecord> CreateBackupAsync(
            string createdBy, string? notes = null, bool includeFiles = true)
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var backupFileName = $"AutoSysDB_{timestamp}.bak";
            var backupFilePath = Path.Combine(_backupDirectory, backupFileName);

            var record = new BackupRecord
            {
                FileName = backupFileName,
                CreatedAt = DateTime.Now,
                CreatedBy = createdBy,
                Notes = notes ?? string.Empty,
                IncludesFiles = includeFiles,
                Type = "Completo",
                Origin = createdBy.Contains("Automático") ? "Automático" : "Manual"
            };

            try
            {
                // ── 1. Backup de la base de datos ──
                var connectionString = _configuration.GetConnectionString("DefaultConnection")!;
                var builder = new SqlConnectionStringBuilder(connectionString);
                var databaseName = builder.InitialCatalog;

                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    var sql = $@"
                        BACKUP DATABASE [{databaseName}]
                        TO DISK = N'{backupFilePath}'
                        WITH FORMAT, INIT,
                             NAME = N'AutoSysDB Backup {timestamp}',
                             SKIP, NOREWIND, NOUNLOAD, STATS = 10";

                    using var command = new SqlCommand(sql, connection);
                    command.CommandTimeout = 600; // 10 minutos máximo
                    await command.ExecuteNonQueryAsync();
                }

                var fileInfo = new FileInfo(backupFilePath);
                record.SizeBytes = fileInfo.Length;

                // ── 2. Backup de archivos subidos (fotos) ──
                if (includeFiles)
                {
                    var uploadsPath = Path.Combine(_env.WebRootPath, "uploads");
                    if (Directory.Exists(uploadsPath) && Directory.GetFiles(uploadsPath, "*", SearchOption.AllDirectories).Length > 0)
                    {
                        var filesZipName = $"AutoSysDB_{timestamp}_files.zip";
                        var filesZipPath = Path.Combine(_backupDirectory, filesZipName);
                        ZipFile.CreateFromDirectory(uploadsPath, filesZipPath, CompressionLevel.Optimal, false);

                        record.FilesZipName = filesZipName;
                        record.SizeBytes += new FileInfo(filesZipPath).Length;
                    }
                }

                record.Status = "Exitoso";
                _logger.LogInformation("Backup creado exitosamente: {FileName} ({SizeBytes} bytes)", backupFileName, record.SizeBytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear backup de base de datos");
                record.Status = "Fallido";
                record.Notes += $" | Error: {ex.Message}";
            }

            // Guardar en catálogo
            var catalog = LoadCatalog();
            catalog.Add(record);
            SaveCatalog(catalog);

            return record;
        }

        // ───────────────────────── CREAR BACKUP DIFERENCIAL ─────────────────────────

        public async Task<BackupRecord> CreateDifferentialBackupAsync(
            string createdBy, string? notes = null)
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var backupFileName = $"AutoSysDB_DIFF_{timestamp}.bak";
            var backupFilePath = Path.Combine(_backupDirectory, backupFileName);

            var record = new BackupRecord
            {
                FileName = backupFileName,
                CreatedAt = DateTime.Now,
                CreatedBy = createdBy,
                Notes = notes ?? string.Empty,
                IncludesFiles = false,
                Type = "Diferencial",
                Origin = createdBy.Contains("Automático") ? "Automático" : "Manual"
            };

            try
            {
                var connectionString = _configuration.GetConnectionString("DefaultConnection")!;
                var builder = new SqlConnectionStringBuilder(connectionString);
                var databaseName = builder.InitialCatalog;

                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    var sql = $@"
                        BACKUP DATABASE [{databaseName}]
                        TO DISK = N'{backupFilePath}'
                        WITH DIFFERENTIAL, FORMAT, INIT,
                             NAME = N'AutoSysDB Diff Backup {timestamp}',
                             SKIP, NOREWIND, NOUNLOAD, STATS = 10";

                    using var command = new SqlCommand(sql, connection);
                    command.CommandTimeout = 600;
                    await command.ExecuteNonQueryAsync();
                }

                var fileInfo = new FileInfo(backupFilePath);
                record.SizeBytes = fileInfo.Length;
                record.Status = "Exitoso";

                _logger.LogInformation("Backup diferencial creado exitosamente: {FileName} ({SizeBytes} bytes)",
                    backupFileName, record.SizeBytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear backup diferencial");
                record.Status = "Fallido";
                record.Notes += $" | Error: {ex.Message}";
            }

            var catalogDiff = LoadCatalog();
            catalogDiff.Add(record);
            SaveCatalog(catalogDiff);

            return record;
        }

        // ───────────────────────── VERIFICAR BACKUP ─────────────────────────

        public async Task<bool> VerifyBackupAsync(string backupId)
        {
            var record = GetBackup(backupId);
            if (record == null) return false;

            var backupFilePath = Path.Combine(_backupDirectory, record.FileName);
            if (!File.Exists(backupFilePath)) return false;

            try
            {
                var connectionString = _configuration.GetConnectionString("DefaultConnection")!;

                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                var sql = $"RESTORE VERIFYONLY FROM DISK = N'{backupFilePath}'";
                using var command = new SqlCommand(sql, connection);
                command.CommandTimeout = 600;
                await command.ExecuteNonQueryAsync();

                // Actualizar estado en catálogo
                record.Status = "Verificado";
                UpdateRecord(record);

                _logger.LogInformation("Backup verificado correctamente: {FileName}", record.FileName);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar backup {BackupId}", backupId);
                return false;
            }
        }

        // ───────────────────────── RESTAURAR BACKUP ─────────────────────────

        public async Task<bool> RestoreBackupAsync(string backupId)
        {
            var record = GetBackup(backupId);
            if (record == null) return false;

            var backupFilePath = Path.Combine(_backupDirectory, record.FileName);
            if (!File.Exists(backupFilePath)) return false;

            try
            {
                var connectionString = _configuration.GetConnectionString("DefaultConnection")!;
                var builder = new SqlConnectionStringBuilder(connectionString);
                var databaseName = builder.InitialCatalog;

                // Construir cadena de conexión al master (necesaria para restore)
                builder.InitialCatalog = "master";
                var masterConnectionString = builder.ConnectionString;

                using var connection = new SqlConnection(masterConnectionString);
                await connection.OpenAsync();

                // 1. Poner la BD en modo single user para cerrar conexiones activas
                var sqlSingleUser = $@"
                    IF EXISTS (SELECT 1 FROM sys.databases WHERE name = '{databaseName}')
                    BEGIN
                        ALTER DATABASE [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                    END";

                using (var cmd = new SqlCommand(sqlSingleUser, connection))
                {
                    cmd.CommandTimeout = 120;
                    await cmd.ExecuteNonQueryAsync();
                }

                // 2. Restaurar la base de datos
                var sqlRestore = $@"
                    RESTORE DATABASE [{databaseName}]
                    FROM DISK = N'{backupFilePath}'
                    WITH REPLACE, STATS = 10";

                using (var cmd = new SqlCommand(sqlRestore, connection))
                {
                    cmd.CommandTimeout = 600;
                    await cmd.ExecuteNonQueryAsync();
                }

                // 3. Volver a multi user
                var sqlMultiUser = $@"
                    ALTER DATABASE [{databaseName}] SET MULTI_USER;";

                using (var cmd = new SqlCommand(sqlMultiUser, connection))
                {
                    cmd.CommandTimeout = 120;
                    await cmd.ExecuteNonQueryAsync();
                }

                // 4. Restaurar archivos si existen
                if (record.IncludesFiles && !string.IsNullOrEmpty(record.FilesZipName))
                {
                    var filesZipPath = Path.Combine(_backupDirectory, record.FilesZipName);
                    if (File.Exists(filesZipPath))
                    {
                        var uploadsPath = Path.Combine(_env.WebRootPath, "uploads");

                        // Limpiar carpeta uploads actual
                        if (Directory.Exists(uploadsPath))
                            Directory.Delete(uploadsPath, true);

                        Directory.CreateDirectory(uploadsPath);
                        ZipFile.ExtractToDirectory(filesZipPath, uploadsPath, true);
                    }
                }

                _logger.LogWarning("Base de datos restaurada desde backup: {FileName}", record.FileName);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al restaurar backup {BackupId}", backupId);

                // Intentar volver al modo multi-user en caso de error
                try
                {
                    var connectionString = _configuration.GetConnectionString("DefaultConnection")!;
                    var builder2 = new SqlConnectionStringBuilder(connectionString);
                    var dbName = builder2.InitialCatalog;
                    builder2.InitialCatalog = "master";

                    using var recoveryConn = new SqlConnection(builder2.ConnectionString);
                    await recoveryConn.OpenAsync();
                    using var recoveryCmd = new SqlCommand(
                        $"ALTER DATABASE [{dbName}] SET MULTI_USER;", recoveryConn);
                    recoveryCmd.CommandTimeout = 60;
                    await recoveryCmd.ExecuteNonQueryAsync();
                }
                catch { /* Mejor esfuerzo */ }

                return false;
            }
        }

        // ───────────────────────── ELIMINAR BACKUP ─────────────────────────

        public Task<bool> DeleteBackupAsync(string backupId)
        {
            var record = GetBackup(backupId);
            if (record == null) return Task.FromResult(false);

            try
            {
                // Eliminar archivo .bak
                var bakPath = Path.Combine(_backupDirectory, record.FileName);
                if (File.Exists(bakPath))
                    File.Delete(bakPath);

                // Eliminar archivo .zip de archivos si existe
                if (!string.IsNullOrEmpty(record.FilesZipName))
                {
                    var zipPath = Path.Combine(_backupDirectory, record.FilesZipName);
                    if (File.Exists(zipPath))
                        File.Delete(zipPath);
                }

                // Eliminar del catálogo
                var catalog = LoadCatalog();
                catalog.RemoveAll(r => r.Id == backupId);
                SaveCatalog(catalog);

                _logger.LogInformation("Backup eliminado: {FileName}", record.FileName);
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar backup {BackupId}", backupId);
                return Task.FromResult(false);
            }
        }

        // ───────────────────────── CONSULTAS ─────────────────────────

        public List<BackupRecord> GetBackupHistory()
        {
            return LoadCatalog().OrderByDescending(r => r.CreatedAt).ToList();
        }

        public BackupRecord? GetBackup(string backupId)
        {
            return LoadCatalog().FirstOrDefault(r => r.Id == backupId);
        }

        public string GetBackupFilePath(string backupId)
        {
            var record = GetBackup(backupId);
            if (record == null) return string.Empty;
            return Path.Combine(_backupDirectory, record.FileName);
        }

        public string GetBackupDirectory() => _backupDirectory;

        // ───────────────────────── CATÁLOGO JSON ─────────────────────────

        private List<BackupRecord> LoadCatalog()
        {
            if (!File.Exists(_catalogPath))
                return new List<BackupRecord>();

            var json = File.ReadAllText(_catalogPath);
            return JsonSerializer.Deserialize<List<BackupRecord>>(json)
                   ?? new List<BackupRecord>();
        }

        private void SaveCatalog(List<BackupRecord> catalog)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(catalog, options);
            File.WriteAllText(_catalogPath, json);
        }

        private void UpdateRecord(BackupRecord updated)
        {
            var catalog = LoadCatalog();
            var index = catalog.FindIndex(r => r.Id == updated.Id);
            if (index >= 0)
            {
                catalog[index] = updated;
                SaveCatalog(catalog);
            }
        }
    }
}
