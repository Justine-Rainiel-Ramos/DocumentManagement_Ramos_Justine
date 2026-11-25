
using Microsoft.AspNetCore.Http;
using DocumentManagement.Exceptions;
using Microsoft.EntityFrameworkCore;
using DocumentManagement.Data;
using DocumentManagement.Models;
using DocumentManagement.DTOs.Responses;
using DocumentManagement.Services.Interfaces;
using System.Reflection.Metadata;

namespace DocumentManagement.Services;

public class DocumentService : IDocumentService
{
    private readonly ApplicationDbContext _context;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<DocumentService> _logger;

    private const long MaxFileSize = 10 * 1024 * 1024; // 10 MB

    private static readonly string[] AllowedExtensions =
    {
        ".pdf",
        ".doc",
        ".docx",
        ".xls",
        ".xlsx",
        ".txt",
        ".png",
        ".jpg",
        ".jpeg"
    };

    public DocumentService(ApplicationDbContext context, IFileStorageService fileStorageService, ILogger<DocumentService> logger)
    {
        _context = context;
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    // Upload document
    public async Task<DocumentResponse> UploadDocumentAsync(IFormFile file, CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
            throw new DocumentValidationException("Please select a file.");

        if (file.Length > MaxFileSize)
            throw new DocumentTooLargeException("File size exceeds the 10 MB limit.");

        var originalFileName = Path.GetFileName(file.FileName);
        var extension = Path.GetExtension(originalFileName);

        if (!AllowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
            throw new UnsupportedDocumentTypeException("File extension is not supported.");

        var storedFileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";

        // Save the actual file first.
        await _fileStorageService.SaveFileAsync(file, storedFileName, cancellationToken);

        var document = new Models.Document
        {
            DocumentId = Guid.NewGuid(),
            FileName = originalFileName,
            StoredFileName = $"{Guid.NewGuid()}{extension}",
            ContentType = file.ContentType,
            FileSize = file.Length,
            UploadDate = DateTime.UtcNow
        };

        try
        {
            // Save the document metadata in SQL Server.
            _context.Documents.Add(document);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            // Database save failed. Attempt to remove the physical file.
            try
            {
                await _fileStorageService.DeleteFileAsync(document.StoredFileName, CancellationToken.None);
            }
            catch (Exception cleanupException)
            {
                _logger.LogError(cleanupException, "Failed to clean up uploaded file {StoredFileName} after database save failure.", document.StoredFileName);
            }
            throw;
        }

        return new DocumentResponse
        {
            DocumentId = document.DocumentId,
            FileName = document.FileName,
            ContentType = document.ContentType,
            FileSize = document.FileSize,
            UploadDate = document.UploadDate
        };
    }

    // Retrieve all documents
    public async Task<IEnumerable<DocumentResponse>> GetDocumentsAsync(CancellationToken cancellationToken)
    { 
        var documents = await _context.Documents.AsNoTracking().OrderByDescending(d => d.UploadDate).ToListAsync(cancellationToken);
        return documents.Select(MapToResponse);
    }

    // Retrieve document by ID
    public async Task<DocumentResponse?> GetDocumentByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var document = await _context.Documents.AsNoTracking().FirstOrDefaultAsync(d => d.DocumentId == id, cancellationToken);
        return document == null ? null : MapToResponse(document);
    }

    // Download document
    public async Task<DocumentFileResponse?> DownloadDocumentAsync(Guid id, CancellationToken cancellationToken)
    {
        var document = await _context.Documents.AsNoTracking().FirstOrDefaultAsync(d => d.DocumentId == id, cancellationToken);

        if (document == null)
            return null;

        var fileStream = await _fileStorageService.GetFileAsync(document.StoredFileName, cancellationToken);

        if (fileStream == null)
            return null;

        return new DocumentFileResponse
        {
            Content = fileStream,
            ContentType = document.ContentType,
            FileName = document.FileName
        };
    }

    // Delete document
    public async Task<bool> DeleteDocumentAsync(Guid id, CancellationToken cancellationToken)
    {
        var document = await _context.Documents.FirstOrDefaultAsync(d => d.DocumentId == id, cancellationToken);

        if (document == null)
            return false;

        _context.Documents.Remove(document);

        await _context.SaveChangesAsync(cancellationToken);

        // Delete the physical file after removing its metadata.
        await _fileStorageService.DeleteFileAsync(document.StoredFileName, cancellationToken);
        return true;
    }

    // Map entity to response DTO
    private static DocumentResponse MapToResponse(Models.Document document)
    {
        return new DocumentResponse
        {
            DocumentId = document.DocumentId,
            FileName = document.FileName,
            ContentType = document.ContentType,
            FileSize = document.FileSize,
            UploadDate = document.UploadDate
        };
    }
}
