namespace AutoSys.Patterns.Singleton
{
    /// Configuración global del sistema - instancia única thread-safe con Lazy<T>
    public sealed class AppConfigurationManager
    {
        private static readonly Lazy<AppConfigurationManager> _instance = 
            new Lazy<AppConfigurationManager>(() => new AppConfigurationManager());

        private readonly Dictionary<string, string> _settings;

        private AppConfigurationManager()
        {
            _settings = new Dictionary<string, string>
            {
                { "SessionTimeout", "15" },
                { "MaxFileSize", "5242880" }, // 5MB
                { "AllowedFileExtensions", ".jpg,.jpeg,.png,.pdf" },
                { "DefaultRole", "Recepcionista" },
                { "MinPasswordLength", "6" },
                { "EnableEmailNotifications", "false" },
                { "ReportsPageSize", "20" },
                { "StockWarningLevel", "10" }
            };
        }

        public static AppConfigurationManager Instance => _instance.Value;

        public string GetSetting(string key)
        {
            return _settings.ContainsKey(key) ? _settings[key] : string.Empty;
        }

        public int GetSettingAsInt(string key, int defaultValue = 0)
        {
            if (_settings.ContainsKey(key) && int.TryParse(_settings[key], out int value))
            {
                return value;
            }
            return defaultValue;
        }

        public bool GetSettingAsBool(string key, bool defaultValue = false)
        {
            if (_settings.ContainsKey(key) && bool.TryParse(_settings[key], out bool value))
            {
                return value;
            }
            return defaultValue;
        }

        public void UpdateSetting(string key, string value)
        {
            if (_settings.ContainsKey(key))
            {
                _settings[key] = value;
            }
            else
            {
                _settings.Add(key, value);
            }
        }

        public Dictionary<string, string> GetAllSettings()
        {
            return new Dictionary<string, string>(_settings);
        }
    }
}
