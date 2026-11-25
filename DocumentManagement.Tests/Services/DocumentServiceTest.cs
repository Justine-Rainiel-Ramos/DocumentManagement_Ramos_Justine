using DocumentManagement.Data;
using DocumentManagement.DTOs.Responses;
using DocumentManagement.Exceptions;
using DocumentManagement.Models;
using DocumentManagement.Services;
using DocumentManagement.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace DocumentManagement.Tests.Services
{
    public class DocumentServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly Mock<IFileStorageService> _fileStorageMock;
        private readonly DocumentService _documentService;

        public DocumentServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            _context = new ApplicationDbContext(options);
            _fileStorageMock = new Mock<IFileStorageService>();
            var loggerMock = new Mock<ILogger<DocumentService>>();

            _documentService = new DocumentService(_context, _fileStorageMock.Object, loggerMock.Object);
        }

        private static IFormFile CreateTestFile(string fileName = "test.pdf", long size = 1024)
        {
            var content = new byte[size];
            var stream = new MemoryStream(content);

            return new FormFile(stream, 0, size, "File", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "application/pdf"
            };
        }

        [Fact]
        public async Task UploadDocumentAsync_ValidFile_ReturnsDocumentResponse()
        {
            // Arrange
            var file = CreateTestFile();
            _fileStorageMock.Setup(x => x.SaveFileAsync(It.IsAny<IFormFile>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            // Act
            var result = await _documentService.UploadDocumentAsync(file, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.NotEqual(Guid.Empty, result.DocumentId);
            Assert.Equal("test.pdf", result.FileName);
            Assert.Equal("application/pdf", result.ContentType);
            Assert.Equal(1024, result.FileSize);

            var savedDocument = await _context.Documents.FirstOrDefaultAsync(d => d.DocumentId == result.DocumentId);

            Assert.NotNull(savedDocument);

            _fileStorageMock.Verify(x => x.SaveFileAsync(file, It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetDocumentByIdAsync_ExistingDocument_ReturnsDocumentResponse()
        {
            // Arrange
            var document = new Document
            {
                DocumentId = Guid.NewGuid(),
                FileName = "test.pdf",
                StoredFileName = "unique-test.pdf",
                ContentType = "application/pdf",
                FileSize = 1024,
                UploadDate = DateTime.UtcNow
            };

            _context.Documents.Add(document);
            await _context.SaveChangesAsync();

            // Act
            var result = await _documentService.GetDocumentByIdAsync(document.DocumentId, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(document.DocumentId, result.DocumentId);
            Assert.Equal("test.pdf", result.FileName);
            Assert.Equal("application/pdf", result.ContentType);
        }

        [Fact]
        public async Task GetDocumentByIdAsync_NonexistentDocument_ReturnsNull()
        {
            // Arrange
            var documentId = Guid.NewGuid();

            // Act
            var result = await _documentService.GetDocumentByIdAsync(documentId, CancellationToken.None);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UploadDocumentAsync_FileExceedsMaxSize_ThrowsDocumentTooLargeException()
        {
            // Arrange
            var file = CreateTestFile("large.pdf", (10 * 1024 * 1024) + 1);

            // Act
            var exception = await Assert.ThrowsAsync<DocumentTooLargeException>(() => _documentService.UploadDocumentAsync(file, CancellationToken.None));

            // Assert
            Assert.Equal("File size exceeds the 10 MB limit.", exception.Message);

            _fileStorageMock.Verify(x => x.SaveFileAsync(
                It.IsAny<IFormFile>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
                Times.Never);

            Assert.Empty(await _context.Documents.ToListAsync());
        }

        [Fact]
        public async Task DeleteDocumentAsync_ExistingDocument_DeletesDocument()
        {
            // Arrange
            var document = new Document
            {
                DocumentId = Guid.NewGuid(),
                FileName = "test.pdf",
                StoredFileName = "unique-test.pdf",
                ContentType = "application/pdf",
                FileSize = 1024,
                UploadDate = DateTime.UtcNow
            };

            _context.Documents.Add(document);
            await _context.SaveChangesAsync();

            _fileStorageMock.Setup(x => x.DeleteFileAsync(document.StoredFileName, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            // Act
            var result = await _documentService.DeleteDocumentAsync(document.DocumentId, CancellationToken.None);

            // Assert
            Assert.True(result);

            var deletedDocument = await _context.Documents.FindAsync(document.DocumentId);

            Assert.Null(deletedDocument);

            _fileStorageMock.Verify(x => x.DeleteFileAsync(document.StoredFileName, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UploadDocumentAsync_UnsupportedExtension_ThrowsException()
        {
            // Arrange
            var file = CreateTestFile("randomgames.exe");

            // Act
            var exception = await Assert.ThrowsAsync<UnsupportedDocumentTypeException>(() => _documentService.UploadDocumentAsync(file, CancellationToken.None));

            // Assert
            Assert.Equal("File extension is not supported.", exception.Message);

            _fileStorageMock.Verify(
                x => x.SaveFileAsync(
                    It.IsAny<IFormFile>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task UploadDocumentAsync_EmptyFile_ThrowsValidationException()
        {
            // Arrange
            var file = CreateTestFile("empty.pdf", 0);

            // Act
            var exception = await Assert.ThrowsAsync<DocumentValidationException>(() => _documentService.UploadDocumentAsync(file, CancellationToken.None));

            // Assert
            Assert.Equal("Please select a file.", exception.Message);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}