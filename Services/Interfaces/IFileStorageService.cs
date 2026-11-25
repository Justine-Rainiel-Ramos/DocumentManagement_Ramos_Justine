namespace DocumentManagement.Services.Interfaces
{
    public interface IFileStorageService
    {
        Task SaveFileAsync(IFormFile file, string storedFileName, CancellationToken cancellationToken);

        Task<Stream?> GetFileAsync(string storedFileName, CancellationToken cancellationToken);

        Task DeleteFileAsync(string storedFileName, CancellationToken cancellationToken);
    }
}