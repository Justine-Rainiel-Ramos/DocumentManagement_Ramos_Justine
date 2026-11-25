using DocumentManagement.Services.Interfaces;

namespace DocumentManagement.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly string _storagePath;

        public FileStorageService(IWebHostEnvironment environment)
        {
            _storagePath = Path.Combine(environment.ContentRootPath, "Storage", "Documents");
            Directory.CreateDirectory(_storagePath);
        }

        public async Task SaveFileAsync(IFormFile file, string fileName, CancellationToken cancellationToken)
        {
            var filePath = GetFilePath(fileName);

            await using var stream = new FileStream(
                filePath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 81920,
                useAsync: true);

            await file.CopyToAsync(stream, cancellationToken);
        }

        public Task<Stream?> GetFileAsync(string fileName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var filePath = GetFilePath(fileName);

            if (!File.Exists(filePath))
            {
                return Task.FromResult<Stream?>(null);
            }

            Stream stream = new FileStream(
                filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 81920,
                useAsync: true);

            return Task.FromResult<Stream?>(stream);
        }

        public Task DeleteFileAsync(string fileName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var filePath = GetFilePath(fileName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            return Task.CompletedTask;
        }

        private string GetFilePath(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName) || !string.Equals(fileName, Path.GetFileName(fileName), StringComparison.Ordinal))
            {
                throw new ArgumentException("Invalid stored filename.", nameof(fileName));
            }

            return Path.Combine(_storagePath, fileName);
        }
    }
}