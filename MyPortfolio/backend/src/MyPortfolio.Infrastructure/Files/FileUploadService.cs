using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using MyPortfolio.Application.Common.Interfaces;
using MyPortfolio.Domain.Entities;

namespace MyPortfolio.Infrastructure.Files;

public class FileUploadService : IFileUploadService
{
    private readonly IWebHostEnvironment _environment;
    private readonly IGenericRepository<UserProfile> _userProfileRepository;
    private readonly ILogger<FileUploadService> _logger;

    private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp", ".svg", ".gif"
    };

    public FileUploadService(
        IWebHostEnvironment environment,
        IGenericRepository<UserProfile> userProfileRepository,
        ILogger<FileUploadService> logger)
    {
        _environment = environment;
        _userProfileRepository = userProfileRepository;
        _logger = logger;
    }

    public async Task<UploadFileResult> UploadImageAsync(
        Stream contentStream,
        string fileName,
        string contentType,
        long length,
        string baseUrl,
        CancellationToken cancellationToken = default)
    {
        if (contentStream == null || length == 0)
        {
            return new UploadFileResult(false, Message: "No file was uploaded.");
        }

        var ext = Path.GetExtension(fileName);
        if (string.IsNullOrWhiteSpace(ext) || !AllowedImageExtensions.Contains(ext))
        {
            return new UploadFileResult(false, Message: $"Unsupported file extension '{ext}'. Allowed: .jpg, .png, .webp, .svg, .gif");
        }

        if (string.IsNullOrWhiteSpace(contentType) || !contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            return new UploadFileResult(false, Message: "Only image files (MIME type image/*) are allowed.");
        }

        if (!await IsValidImageContentAsync(contentStream, ext, cancellationToken))
        {
            return new UploadFileResult(false, Message: "The uploaded file is not a valid image format or is corrupted.");
        }

        try
        {
            var webRoot = _environment.WebRootPath;
            if (string.IsNullOrEmpty(webRoot))
            {
                webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            }

            var uploadsFolder = Path.Combine(webRoot, "uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = $"{Guid.NewGuid():N}{ext.ToLowerInvariant()}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            if (contentStream.CanSeek) contentStream.Position = 0;

            await using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await contentStream.CopyToAsync(fileStream, cancellationToken);
            }

            var relativeUrl = $"/uploads/{uniqueFileName}";
            var absoluteUrl = $"{baseUrl.TrimEnd('/')}{relativeUrl}";

            _logger.LogInformation("Image uploaded successfully: {FileName} ({Bytes} bytes)", uniqueFileName, length);

            return new UploadFileResult(
                Succeeded: true,
                Url: absoluteUrl,
                RelativeUrl: relativeUrl,
                FileName: uniqueFileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload image.");
            return new UploadFileResult(false, Message: "Failed to upload image.");
        }
    }

    public async Task<UploadFileResult> UploadCvAsync(
        Stream contentStream,
        string fileName,
        string contentType,
        long length,
        string? userId,
        string baseUrl,
        CancellationToken cancellationToken = default)
    {
        if (contentStream == null || length == 0)
            return new UploadFileResult(false, Message: "A CV PDF file is required.");

        if (!string.Equals(Path.GetExtension(fileName), ".pdf", StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(contentType, "application/pdf", StringComparison.OrdinalIgnoreCase))
            return new UploadFileResult(false, Message: "Only PDF files are supported.");

        if (length > 10 * 1024 * 1024)
            return new UploadFileResult(false, Message: "CV file must be 10 MB or smaller.");

        var webRoot = _environment.WebRootPath;
        if (string.IsNullOrWhiteSpace(webRoot)) webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var uploadDirectory = Path.Combine(webRoot, "uploads", "cv");
        Directory.CreateDirectory(uploadDirectory);
        var uniqueFileName = $"{Guid.NewGuid():N}.pdf";
        var filePath = Path.Combine(uploadDirectory, uniqueFileName);

        if (contentStream.CanSeek) contentStream.Position = 0;

        await using (var fileStream = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
        {
            await contentStream.CopyToAsync(fileStream, cancellationToken);
        }

        await using (var verifyStream = File.OpenRead(filePath))
        {
            var header = new byte[5];
            var read = await verifyStream.ReadAsync(header.AsMemory(0, header.Length), cancellationToken);
            if (read != header.Length || !header.AsSpan().SequenceEqual("%PDF-"u8))
            {
                File.Delete(filePath);
                return new UploadFileResult(false, Message: "The uploaded file is not a valid PDF document.");
            }
        }

        var relativeUrl = $"/uploads/cv/{uniqueFileName}";

        var profile = !string.IsNullOrWhiteSpace(userId)
            ? await _userProfileRepository.FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken)
            : await _userProfileRepository.FirstOrDefaultAsync(p => true, cancellationToken);

        if (profile != null)
        {
            profile.SetCvUrl(relativeUrl);
            _userProfileRepository.Update(profile);
            await _userProfileRepository.SaveChangesAsync(cancellationToken);
        }

        return new UploadFileResult(
            Succeeded: true,
            Url: $"{baseUrl.TrimEnd('/')}{relativeUrl}",
            RelativeUrl: relativeUrl,
            FileName: uniqueFileName);
    }

    private static async Task<bool> IsValidImageContentAsync(Stream stream, string ext, CancellationToken cancellationToken)
    {
        try
        {
            if (stream.CanSeek) stream.Position = 0;
            var header = new byte[16];
            var bytesRead = await stream.ReadAsync(header.AsMemory(0, header.Length), cancellationToken);
            if (bytesRead < 4) return false;

            var lowerExt = ext.ToLowerInvariant();
            switch (lowerExt)
            {
                case ".jpg":
                case ".jpeg":
                    return header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF;

                case ".png":
                    return bytesRead >= 8 &&
                           header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47 &&
                           header[4] == 0x0D && header[5] == 0x0A && header[6] == 0x1A && header[7] == 0x0A;

                case ".gif":
                    return bytesRead >= 6 &&
                           header[0] == 'G' && header[1] == 'I' && header[2] == 'F' && header[3] == '8' &&
                           (header[4] == '7' || header[4] == '9') && header[5] == 'a';

                case ".webp":
                    return bytesRead >= 12 &&
                           header[0] == 'R' && header[1] == 'I' && header[2] == 'F' && header[3] == 'F' &&
                           header[8] == 'W' && header[9] == 'E' && header[10] == 'B' && header[11] == 'P';

                case ".svg":
                    if (stream.CanSeek) stream.Position = 0;
                    using (var reader = new StreamReader(stream, leaveOpen: true))
                    {
                        var content = await reader.ReadToEndAsync(cancellationToken);
                        if (string.IsNullOrWhiteSpace(content)) return false;
                        if (content.Contains("<script", StringComparison.OrdinalIgnoreCase)) return false;
                        return content.Contains("<svg", StringComparison.OrdinalIgnoreCase);
                    }

                default:
                    return false;
            }
        }
        catch
        {
            return false;
        }
    }
}
