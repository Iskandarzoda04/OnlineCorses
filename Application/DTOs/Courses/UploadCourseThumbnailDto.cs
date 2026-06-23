using Microsoft.AspNetCore.Http;

namespace Application.DTOs.Courses;

public class UploadCourseThumbnailDto
{
    public IFormFile File { get; set; } = null!;
}
