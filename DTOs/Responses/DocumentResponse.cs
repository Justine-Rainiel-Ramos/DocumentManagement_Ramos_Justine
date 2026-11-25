
namespace DocumentManagement.DTOs.Responses;

public class DocumentResponse
{
    public Guid DocumentId { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long FileSize { get; set; }

    public DateTime UploadDate { get; set; }
}
