using Microsoft.AspNetCore.Http;
using Application.Common;
using Application.DTOs.Courses;

namespace Application.Interfaces.Services;

public interface ICourseService
{
    Task<Result<List<CourseDto>>> GetAsync(CourseQueryDto query);
    Task<Result<CourseDto>> GetByIdAsync(Guid Id);
    Task<Result<CourseDto>> CreateAsync(string instructorId, CreateCourseDto dto);
    Task<Result<CourseDto>> UpdateAsync(Guid Id, string userId, bool isAdmin, UpdateCourseDto dto);
    Task<Result> DeleteAsync(Guid Id, string userId, bool isAdmin);
    Task<Result<CourseDto>> PublishAsync(Guid Id, string userId, bool isAdmin);
    Task<Result<CourseDto>> UploadThumbnailAsync(Guid Id, string userId, bool isAdmin, IFormFile file);
}
