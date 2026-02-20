using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace AutoSys.Services
{
    public class LocalFileStorage : IFileStorage
    {
        private readonly IWebHostEnvironment _env;
        private readonly HashSet<string> _allowedExtensions;
        private readonly long _maxFileSizeBytes;

        // Extensiones de imagen permitidas por defecto
        private static readonly HashSet<string> DefaultAllowed =
            new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

        public LocalFileStorage(IWebHostEnvironment env, IConfiguration configuration)
        {
            _env = env;

            var configExts = configuration["FileUpload:AllowedExtensions"];
            _allowedExtensions = string.IsNullOrWhiteSpace(configExts)
                ? DefaultAllowed
                : new HashSet<string>(
                    configExts.Split(',', StringSplitOptions.RemoveEmptyEntries)
                              .Select(e => e.Trim().ToLowerInvariant()),
                    StringComparer.OrdinalIgnoreCase);

            _maxFileSizeBytes = configuration.GetValue<long>("FileUpload:MaxFileSizeBytes", 5_242_880); // 5 MB
        }

        public async Task<string> SaveAsync(Stream fileStream, string fileName, string contentType)
        {
            // Validar extensión
            var ext = Path.GetExtension(fileName);
            if (string.IsNullOrEmpty(ext) || !_allowedExtensions.Contains(ext))
                throw new InvalidOperationException($"Tipo de archivo no permitido: '{ext}'. Solo se aceptan: {string.Join(", ", _allowedExtensions)}");

            // Validar tamaño
            if (fileStream.Length > _maxFileSizeBytes)
                throw new InvalidOperationException($"El archivo supera el tamaño máximo de {_maxFileSizeBytes / 1_048_576} MB.");

            // Guardar con nombre aleatorio (nunca usar el nombre original del usuario)
            string folder = Path.Combine(_env.WebRootPath, "uploads");
            Directory.CreateDirectory(folder);
            string safeFileName = Guid.NewGuid().ToString() + ext.ToLowerInvariant();
            string path = Path.Combine(folder, safeFileName);

            using var fs = new FileStream(path, FileMode.Create);
            await fileStream.CopyToAsync(fs);

            return "/uploads/" + safeFileName;
        }

        public Task<bool> MoveAsync(string tempPath, string finalFolder)
        {
            try
            {
                string webRoot = _env.WebRootPath;

                // Sanitizar paths para prevenir path traversal
                string safeTempPath    = SanitizePath(tempPath);
                string safeFinalFolder = SanitizePath(finalFolder);

                string tempFull = Path.GetFullPath(Path.Combine(webRoot, safeTempPath));
                string finalDir = Path.GetFullPath(Path.Combine(webRoot, safeFinalFolder));

                // Asegurar que el destino siga dentro de webRoot
                if (!tempFull.StartsWith(webRoot, StringComparison.OrdinalIgnoreCase) ||
                    !finalDir.StartsWith(webRoot, StringComparison.OrdinalIgnoreCase))
                    return Task.FromResult(false);

                // Validar extensión del archivo destino
                string fileExt = Path.GetExtension(tempFull);
                if (!_allowedExtensions.Contains(fileExt))
                    return Task.FromResult(false);

                Directory.CreateDirectory(finalDir);
                string fileName = Path.GetFileName(tempFull);
                string dest     = Path.Combine(finalDir, fileName);

                if (File.Exists(tempFull))
                {
                    File.Move(tempFull, dest);
                    return Task.FromResult(true);
                }
                return Task.FromResult(false);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }

        private static string SanitizePath(string path) =>
            path.TrimStart('/', '\\')
                .Replace("..", string.Empty)
                .Replace('/', Path.DirectorySeparatorChar);
    }
}
