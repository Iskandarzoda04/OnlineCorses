using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Application.Interfaces.Services;

namespace Infrastructure.FileStorage;

public class FileService(IConfiguration configuration) : IFileService
{
    public async Task<string> SaveCourseThumbnailAsync(Guid courseId, IFormFile file)
    {
        var root = configuration["FileStorage:UploadPath"] ?? configuration["FileStorage:RootPath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "thumbnails");
        var publicBase = configuration["FileStorage:PublicBasePath"] ?? "/uploads/thumbnails";
        Directory.CreateDirectory(root);

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (extension is not (".jpg" or ".jpeg" or ".png"))
            throw new InvalidOperationException("Only jpeg and png files are allowed.");

        var fileName = $"{courseId}_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}{extension}";
        var path = Path.Combine(root, fileName);

        await using var stream = File.Create(paath);
        await file.CopyToAsync(stream);
        return $"{publicBase.TrimEnd('/')}/{fileName}";
    }
}
