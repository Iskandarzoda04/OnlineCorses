using Microsoft.AspNetCore.Http;
using Application.Common;
using Application.DTOs.Courses;

namespace Application.Interfaces.Services;

public interface ICourseService
{
    Task<Result<List<CourseDto>>> GetAsync(CourseQueryDto query);
    Task<Result<CourseDto>> GetByIdAsync(Guid Id);
    Task<Result<GetAllCourseDto>> GetAll(Guid Id);
    Task<Result<CourseDto>> CreateAsync( CreateCourseDto dto);
    Task<Result<CourseDto>> CreateAsync(string userId, CreateCourseDto dto);
    Task<Result<CourseDto>> UpdateAsync(Guid Id,  UpdateCourseDto dto);
    Task<Result> DeleteAsync(Guid Id, string userId, bool isAdmin);
    Task<Result<CourseDto>> PublishAsync(Guid Id, string userId, bool isAdmin);
    Task<Result<CourseDto>> UploadThumbnailAsync(Guid Id, string userId, bool isAdmin, IFormFile file);
    Task<Result<List<CourseDto>>> GetAllCourseAsync();
}
