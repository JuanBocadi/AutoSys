using System.Text.Json;

namespace AutoSys.Services
{
    /// <summary>
    /// Estado persistido del scheduler de backups automáticos.
    /// Se guarda en un archivo JSON junto al catálogo de backups.
    /// </summary>
    public class BackupScheduleState
    {
        public bool Enabled { get; set; } = true;
        public DateTime? NextFullBackup { get; set; }
        public DateTime? NextDiffBackup { get; set; }
        public DateTime? LastAutoFullBackup { get; set; }
        public DateTime? LastAutoDiffBackup { get; set; }
    }

    /// <summary>
    /// Servicio en segundo plano que programa y ejecuta backups automáticos.
    /// 
    /// - Backup Completo: cada X horas (configurable, por defecto 24h)
    /// - Backup Diferencial: cada Y horas (configurable, por defecto 6h)
    /// - El cronograma automático es independiente de los backups manuales.
    /// - Se puede habilitar/deshabilitar desde la interfaz web.
    /// 
    /// Utiliza IServiceScopeFactory para crear scopes de DI y obtener IBackupService.
    /// </summary>
    public class BackupSchedulerService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<BackupSchedulerService> _logger;
        private readonly string _statePath;
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1);

        // Intervalos configurables
        private readonly int _fullIntervalHours;
        private readonly int _diffIntervalHours;

        private static readonly object _lock = new();

        public BackupSchedulerService(
            IServiceScopeFactory scopeFactory,
            IConfiguration configuration,
            ILogger<BackupSchedulerService> logger,
            IWebHostEnvironment env)
        {
            _scopeFactory = scopeFactory;
            _configuration = configuration;
            _logger = logger;

            // Leer configuración
            _fullIntervalHours = configuration.GetValue("Backup:Schedule:FullIntervalHours", 24);
            _diffIntervalHours = configuration.GetValue("Backup:Schedule:DiffIntervalHours", 6);

            var backupDir = configuration["Backup:Directory"];
            if (string.IsNullOrEmpty(backupDir))
                backupDir = Path.Combine(env.ContentRootPath, "Backups");

            if (!Directory.Exists(backupDir))
                Directory.CreateDirectory(backupDir);

            _statePath = Path.Combine(backupDir, "scheduler_state.json");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "BackupScheduler iniciado. Completo cada {Full}h, Diferencial cada {Diff}h",
                _fullIntervalHours, _diffIntervalHours);

            // Inicializar estado si no existe
            var state = LoadState();
            if (state.NextFullBackup == null)
            {
                state.NextFullBackup = DateTime.Now.AddHours(_fullIntervalHours);
                state.NextDiffBackup = DateTime.Now.AddHours(_diffIntervalHours);
                SaveState(state);
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(_checkInterval, stoppingToken);

                    state = LoadState();
                    if (!state.Enabled)
                        continue;

                    var now = DateTime.Now;

                    // ── Verificar backup COMPLETO ──
                    if (state.NextFullBackup.HasValue && now >= state.NextFullBackup.Value)
                    {
                        _logger.LogInformation("Ejecutando backup COMPLETO automático programado...");
                        await ExecuteAutomaticBackupAsync("Completo");

                        state = LoadState(); // re-read in case changed
                        state.LastAutoFullBackup = DateTime.Now;
                        state.NextFullBackup = DateTime.Now.AddHours(_fullIntervalHours);
                        // Después de un full, resetear el diferencial
                        state.NextDiffBackup = DateTime.Now.AddHours(_diffIntervalHours);
                        SaveState(state);

                        _logger.LogInformation(
                            "Backup completo automático completado. Próximo: {Next}",
                            state.NextFullBackup?.ToString("dd/MM/yyyy HH:mm"));
                    }
                    // ── Verificar backup DIFERENCIAL ──
                    else if (state.NextDiffBackup.HasValue && now >= state.NextDiffBackup.Value)
                    {
                        // Solo hacer diferencial si hay al menos un full previo
                        using var scope = _scopeFactory.CreateScope();
                        var backupService = scope.ServiceProvider.GetRequiredService<IBackupService>();
                        var history = backupService.GetBackupHistory();
                        var hasFullBackup = history.Any(b => b.Type == "Completo" && b.Status != "Fallido");

                        if (hasFullBackup)
                        {
                            _logger.LogInformation("Ejecutando backup DIFERENCIAL automático programado...");
                            await ExecuteAutomaticBackupAsync("Diferencial");
                        }
                        else
                        {
                            _logger.LogInformation("No hay backup completo base. Ejecutando completo en su lugar...");
                            await ExecuteAutomaticBackupAsync("Completo");
                            state = LoadState();
                            state.LastAutoFullBackup = DateTime.Now;
                            state.NextFullBackup = DateTime.Now.AddHours(_fullIntervalHours);
                        }

                        state = LoadState();
                        state.LastAutoDiffBackup = DateTime.Now;
                        state.NextDiffBackup = DateTime.Now.AddHours(_diffIntervalHours);
                        SaveState(state);

                        _logger.LogInformation(
                            "Backup diferencial automático completado. Próximo: {Next}",
                            state.NextDiffBackup?.ToString("dd/MM/yyyy HH:mm"));
                    }
                }
                catch (OperationCanceledException)
                {
                    // Shutdown solicitado
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error en el scheduler de backups automáticos");
                    // Esperar un poco más antes de reintentar
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
            }

            _logger.LogInformation("BackupScheduler detenido.");
        }

        private async Task ExecuteAutomaticBackupAsync(string type)
        {
            using var scope = _scopeFactory.CreateScope();
            var backupService = scope.ServiceProvider.GetRequiredService<IBackupService>();

            if (type == "Diferencial")
            {
                await backupService.CreateDifferentialBackupAsync("Sistema (Automático)", "Backup diferencial automático programado");
            }
            else
            {
                await backupService.CreateBackupAsync("Sistema (Automático)", "Backup completo automático programado", true);
            }
        }

        // ───────── Estado persistido ─────────

        public BackupScheduleState LoadState()
        {
            lock (_lock)
            {
                if (!File.Exists(_statePath))
                    return new BackupScheduleState();

                try
                {
                    var json = File.ReadAllText(_statePath);
                    return JsonSerializer.Deserialize<BackupScheduleState>(json) ?? new BackupScheduleState();
                }
                catch
                {
                    return new BackupScheduleState();
                }
            }
        }

        public void SaveState(BackupScheduleState state)
        {
            lock (_lock)
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(state, options);
                File.WriteAllText(_statePath, json);
            }
        }

        /// <summary>
        /// Habilita o deshabilita el scheduler. Si se habilita, recalcula los próximos backups.
        /// </summary>
        public void SetEnabled(bool enabled)
        {
            var state = LoadState();
            state.Enabled = enabled;

            if (enabled)
            {
                // Recalcular próximos backups desde ahora
                state.NextFullBackup = DateTime.Now.AddHours(_fullIntervalHours);
                state.NextDiffBackup = DateTime.Now.AddHours(_diffIntervalHours);
            }

            SaveState(state);
            _logger.LogInformation("Backup automático {Status}", enabled ? "HABILITADO" : "DESHABILITADO");
        }

        public int FullIntervalHours => _fullIntervalHours;
        public int DiffIntervalHours => _diffIntervalHours;
    }
}
