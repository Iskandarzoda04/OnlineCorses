using Application.Common;
using Application.DTOs.Courses;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class CourseService : ICourseService
{
    private readonly ICourseRepository _courseRepository;
    private readonly IFileService _fileService;
    private readonly ICacheService _cache;
    private readonly ILogger<CourseService> _logger;

    public CourseService(
        ICourseRepository courseRepository,
        IFileService fileService,
        ICacheService cache,
        ILogger<CourseService> logger)
    {
        _courseRepository = courseRepository;
        _fileService = fileService;
        _cache = cache;
        _logger = logger;
    }

    public async Task<Result<List<CourseDto>>> GetAsync(CourseQueryDto query)
    {
        if (query.Page < 1)
            return Result<List<CourseDto>>.Failure("Page must be at least 1.", ErrorType.Validation);

        if (query.PageSize < 1 || query.PageSize > 50)
            return Result<List<CourseDto>>.Failure("PageSize must be between 1 and 50.", ErrorType.Validation);

        var items = await _courseRepository.GetAsync(query);
        var result = items.Select(MapToDto).ToList();

        return Result<List<CourseDto>>.Success(result);
    }

    public async Task<Result<CourseDto>> GetByIdAsync(Guid Id)
    {
        var course = await _courseRepository.GetByIdAsync(Id);

        if (course is null)
            return Result<CourseDto>.Failure("Course not found.", ErrorType.NotFound);

        return Result<CourseDto>.Success(MapToDto(course));
    }

    public async Task<Result<CourseDto>> CreateAsync( CreateCourseDto dto)
    {
        return await CreateAsync(string.Empty, dto);
    }

    public async Task<Result<CourseDto>> CreateAsync(string userId, CreateCourseDto dto)
    {
        
        if (string.IsNullOrWhiteSpace(dto.Title))
            return Result<CourseDto>.Failure("Course title is required.", ErrorType.Validation);

        if (dto.Title.Length > 200)
            return Result<CourseDto>.Failure("Course title max length is 200 characters.", ErrorType.Validation);

        if (dto.Price < 0)
            return Result<CourseDto>.Failure("Course price cannot be negative.", ErrorType.Validation);

        var category = await _courseRepository.GetCategoryAsync(dto.CategoryId);
        if (category is null)
            return Result<CourseDto>.Failure("Category not found.", ErrorType.NotFound);

        var course = new Course
        {
            Title = dto.Title.Trim(),
            Description = dto.Description,
            Price = dto.Price,
            Level = dto.Level,
            CategoryId = dto.CategoryId,
            InstructorId = userId,
        
        };

        await _courseRepository.CreateAsync(course);
        await _cache.RemoveAsync("dashboard:summary");
        await _cache.RemoveAsync("dashboard:top-courses");

        _logger.LogInformation("Course {CourseId} created", course.Id);

        var createdCourse = await _courseRepository.GetByIdAsync(course.Id);

        return Result<CourseDto>.Success(MapToDto(createdCourse ?? course));
    }

    public async Task<Result<CourseDto>> UpdateAsync(Guid Id,UpdateCourseDto dto)
    {
        var course = await _courseRepository.GetByIdForUpdateAsync(Id);

        if (course is null)
            return Result<CourseDto>.Failure("Course not found.", ErrorType.NotFound);

      

        if (string.IsNullOrWhiteSpace(dto.Title))
            return Result<CourseDto>.Failure("Course title is required.", ErrorType.Validation);

        if (dto.Title.Length > 200)
            return Result<CourseDto>.Failure("Course title max length is 200 characters.", ErrorType.Validation);

        if (dto.Price < 0)
            return Result<CourseDto>.Failure("Course price cannot be negative.", ErrorType.Validation);

        var category = await _courseRepository.GetCategoryAsync(dto.CategoryId);
        if (category is null)
            return Result<CourseDto>.Failure("Category not found.", ErrorType.NotFound);

        course.Title = dto.Title.Trim();
        course.Description = dto.Description;
        course.Price = dto.Price;
        course.Level = dto.Level;
        course.CategoryId = dto.CategoryId;
        course.Category = category;
        course.UpdatedAt = DateTime.UtcNow;

        await _courseRepository.UpdateAsync(course);
        await _cache.RemoveAsync("dashboard:summary");
        await _cache.RemoveAsync("dashboard:top-courses");

        return Result<CourseDto>.Success(MapToDto(course));
    }

    public async Task<Result> DeleteAsync(Guid Id, string userId, bool isAdmin)
    {
        var course = await _courseRepository.GetByIdForUpdateAsync(Id);
        if (course is null)
            return Result.Failure("Course not found.", ErrorType.NotFound);

        if (!isAdmin && course.InstructorId != userId)
            return Result.Failure("You can delete only your own courses.", ErrorType.Forbidden);

        await _courseRepository.DeleteAsync(course);
        await _cache.RemoveAsync("dashboard:summary");
        await _cache.RemoveAsync("dashboard:top-courses");

        return Result.Success("Course deleted successfully.");
    }

    public async Task<Result<CourseDto>> PublishAsync(Guid Id, string userId, bool isAdmin)
    {
        var course = await _courseRepository.GetByIdForUpdateAsync(Id);

        if (course is null)
            return Result<CourseDto>.Failure("Course not found.", ErrorType.NotFound);

        if (!isAdmin && course.InstructorId != userId)
            return Result<CourseDto>.Failure("You can publish only your own courses.", ErrorType.Forbidden);

        course.IsPublished = !course.IsPublished;
        course.PublishedAt = course.IsPublished ? DateTime.UtcNow : null;
        course.UpdatedAt = DateTime.UtcNow;

        await _courseRepository.UpdateAsync(course);
        await _cache.RemoveAsync("dashboard:summary");
        await _cache.RemoveAsync("dashboard:top-courses");

        return Result<CourseDto>.Success(MapToDto(course));
    }

    public async Task<Result<CourseDto>> UploadThumbnailAsync(Guid Id, string userId, bool isAdmin, IFormFile file)
    {
        var course = await _courseRepository.GetByIdForUpdateAsync(Id);

        if (course is null)
            return Result<CourseDto>.Failure("Course not found.", ErrorType.NotFound);

        if (!isAdmin && course.InstructorId != userId)
            return Result<CourseDto>.Failure("You can update only your own courses.", ErrorType.Forbidden);

        if (file is null)
            return Result<CourseDto>.Failure("Thumbnail file is required.", ErrorType.Validation);

        if (file.Length <= 0 || file.Length > 5 * 1024 * 1024)
            return Result<CourseDto>.Failure("Thumbnail max size is 5MB.", ErrorType.Validation);

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if ((file.ContentType != "image/jpeg" && file.ContentType != "image/png") || extension is not (".jpg" or ".jpeg" or ".png"))
            return Result<CourseDto>.Failure("Thumbnail must be jpeg or png.", ErrorType.Validation);

        course.ThumbnailUrl = await _fileService.SaveCourseThumbnailAsync(course.Id, file);
        course.UpdatedAt = DateTime.UtcNow;

        await _courseRepository.UpdateAsync(course);
        await _cache.RemoveAsync("dashboard:summary");
        await _cache.RemoveAsync("dashboard:top-courses");

        return Result<CourseDto>.Success(MapToDto(course));
    }

    private static CourseDto MapToDto(Course course)
    {
        return new CourseDto
        {
            Id = course.Id,
            Title = course.Title,
            Description = course.Description,
            Price = course.Price,
            Level = course.Level,
            IsPublished = course.IsPublished,
            ThumbnailUrl = course.ThumbnailUrl,
            InstructorId = course.InstructorId,
            InstructorName = course.Instructor?.FullName ?? "",
            CategoryId = course.CategoryId,
            CategoryName = course.Category?.Name ?? "",
            AverageRating = course.Reviews.Count == 0 ? 0 : course.Reviews.Average(r => r.Rating),
            StudentsCount = course.Enrollments.Count,
            LessonsCount = course.Lessons.Count,
            ReviewsCount = course.Reviews.Count,
            EnrollmentsCount = course.Enrollments.Count,
            CreatedAt = course.CreatedAt
        };
    }

    public async Task<Result<List<CourseDto>>> GetAllCourseAsync()
    {
        var courses = await _courseRepository.GetAllAsync();
        return Result<List<CourseDto>>.Success(courses.Select(MapToDto).ToList());
    }

    public Task<Result<GetAllCourseDto>> GetAll(Guid Id)
    {
        throw new NotImplementedException();
    }

}
