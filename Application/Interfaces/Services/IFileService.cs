using Microsoft.AspNetCore.Http;

namespace Application.Interfaces.Services;

public interface IFileService
{
    Task<string> SaveCourseThumbnailAsync(Guid courseId, IFormFile file);
}
