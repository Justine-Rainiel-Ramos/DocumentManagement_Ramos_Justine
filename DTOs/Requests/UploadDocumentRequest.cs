
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace DocumentManagement.DTOs.Requests;

public class UploadDocumentRequest
{
    [Required(ErrorMessage = "Please upload a document.")]
    public IFormFile File { get; set; } = null!;
}
