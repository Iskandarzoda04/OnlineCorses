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

        var extension = file.ContentType == "image/png" ? ".png" : ".jpg";
        var fileName = $"{courseId}_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}{extension}";
        var path = Path.Combine(root, fileName);

        await using var stream = File.Create(path);
        await file.CopyToAsync(stream);
        return $"{publicBase.TrimEnd('/')}/{fileName}";
    }
}
