using Microsoft.AspNetCore.Hosting;

namespace AutoSys.Services
{
    public class LocalFileStorage : IFileStorage
    {
        private readonly IWebHostEnvironment _env;

        public LocalFileStorage(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<string> SaveAsync(Stream fileStream, string fileName, string contentType)
        {
            string folder = Path.Combine(_env.WebRootPath, "uploads");
            Directory.CreateDirectory(folder);
            string name = Guid.NewGuid().ToString() + Path.GetExtension(fileName);
            string path = Path.Combine(folder, name);
            using (var fs = new FileStream(path, FileMode.Create))
            {
                await fileStream.CopyToAsync(fs);
            }
            return "/uploads/" + name;
        }

        public Task<bool> MoveAsync(string tempPath, string finalFolder)
        {
            try
            {
                string webRoot = _env.WebRootPath;
                string tempFull = Path.Combine(webRoot, tempPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                string finalDir = Path.Combine(webRoot, finalFolder.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                Directory.CreateDirectory(finalDir);
                string fileName = Path.GetFileName(tempFull);
                string dest = Path.Combine(finalDir, fileName);
                if (System.IO.File.Exists(tempFull))
                {
                    System.IO.File.Move(tempFull, dest);
                    return Task.FromResult(true);
                }
                return Task.FromResult(false);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }
    }
}


