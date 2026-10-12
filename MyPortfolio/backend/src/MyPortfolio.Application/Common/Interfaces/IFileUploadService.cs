namespace MyPortfolio.Application.Common.Interfaces;

public record UploadFileResult(
    bool Succeeded,
    string? Url = null,
    string? RelativeUrl = null,
    string? FileName = null,
    string? Message = null);

public interface IFileUploadService
{
    Task<UploadFileResult> UploadImageAsync(
        Stream contentStream,
        string fileName,
        string contentType,
        long length,
        string baseUrl,
        CancellationToken cancellationToken = default);

    Task<UploadFileResult> UploadCvAsync(
        Stream contentStream,
        string fileName,
        string contentType,
        long length,
        string? userId,
        string baseUrl,
        CancellationToken cancellationToken = default);
}
