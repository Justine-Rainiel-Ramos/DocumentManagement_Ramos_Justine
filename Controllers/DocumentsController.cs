
using DocumentManagement.DTOs.Requests;
using DocumentManagement.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DocumentManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentsController : ControllerBase
    {
        private readonly IDocumentService _documentService;

        public DocumentsController(IDocumentService documentService)
        {
            _documentService = documentService;
        }

        // POST: api/documents
        [HttpPost]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(11 * 1024 * 1024)]
        public async Task<IActionResult> UploadDocument([FromForm] UploadDocumentRequest request, CancellationToken cancellationToken)
        {
            var document = await _documentService.UploadDocumentAsync(request.File, cancellationToken);

            return CreatedAtAction(nameof(GetDocument), new { id = document.DocumentId }, document);
        }

        // GET: api/documents
        [HttpGet]
        public async Task<IActionResult> GetDocuments(CancellationToken cancellationToken)
        {
            var documents = await _documentService.GetDocumentsAsync(cancellationToken);
            if(documents == null)
            {
                return NotFound(new
                {
                    message = "Document does not exist"
                });
            }
            return Ok(documents);
        }

        // GET: api/documents/{id}
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetDocument(Guid id, CancellationToken cancellationToken)
        {
            var document = await _documentService.GetDocumentByIdAsync(id, cancellationToken);

            if (document == null)
            {
                return NotFound(new
                {
                    message = "Document does not exist"
                });
            }
            return Ok(document);
        }

        // GET: api/documents/{id}/download
        [HttpGet("{id:guid}/download")]
        public async Task<IActionResult> DownloadDocument(Guid id, CancellationToken cancellationToken)
        {
            var document = await _documentService.DownloadDocumentAsync(id, cancellationToken);

            if (document == null)
            {
                return NotFound(new
                {
                    message = "Document does not exist"
                });
            }
            return File(document.Content, document.ContentType, document.FileName);
        }

        // DELETE: api/documents/{id}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteDocument(Guid id, CancellationToken cancellationToken)
        {
            var deleted = await _documentService.DeleteDocumentAsync(id, cancellationToken);

            if (!deleted)
            {
                return NotFound(new
                {
                    message = "Document does not exist"
                });
            }
            return NoContent();
        }
    }
}