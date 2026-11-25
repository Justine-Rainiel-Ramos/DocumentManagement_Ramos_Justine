
namespace DocumentManagement.DTOs.Responses;

public class DocumentFileResponse
{
    public Stream Content { get; set; } = Stream.Null;

    public string ContentType { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;
}
