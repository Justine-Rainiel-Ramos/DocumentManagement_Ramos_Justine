
using Microsoft.AspNetCore.Http;
using DocumentManagement.DTOs.Responses;

namespace DocumentManagement.Services.Interfaces
{
    public interface IDocumentService
    {
        Task<DocumentResponse> UploadDocumentAsync(IFormFile file, CancellationToken cancellationToken);

        Task<IEnumerable<DocumentResponse>> GetDocumentsAsync(CancellationToken cancellationToken);

        Task<DocumentResponse?> GetDocumentByIdAsync(Guid id, CancellationToken cancellationToken);

        Task<DocumentFileResponse?> DownloadDocumentAsync(Guid id, CancellationToken cancellationToken);

        Task<bool> DeleteDocumentAsync(Guid id, CancellationToken cancellationToken);
    }
}