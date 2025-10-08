namespace AutoSys.Services
{
    public interface IFileStorage
    {
        Task<string> SaveAsync(Stream fileStream, string fileName, string contentType);
        Task<bool> MoveAsync(string tempPath, string finalFolder);
    }
}


