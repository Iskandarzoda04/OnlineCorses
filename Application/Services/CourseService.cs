using Application.Common;
using Application.DTOs.Courses;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class CourseService : ICourseService
{
    private static readonly List<CourseDto> Courses = new();
    private readonly IFileService _fileService;
    private readonly ILogger<CourseService> _logger;

    public CourseService(
        IFileService fileService,
        ILogger<CourseService> logger)
    {
        _fileService = fileService;
        _logger = logger;
    }

    public Task<Result<List<CourseDto>>> GetAsync(CourseQueryDto query)
    {
        if (query.Page < 1)
            return Task.FromResult(Result<List<CourseDto>>.Failure("Page must be at least 1.", ErrorType.Validation));

        if (query.PageSize < 1 || query.PageSize > 50)
            return Task.FromResult(Result<List<CourseDto>>.Failure("PageSize must be between 1 and 50.", ErrorType.Validation));

        var courses = Courses.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.ToLower();
            courses = courses.Where(c =>
                c.Title.ToLower().Contains(search) ||
                (c.Description != null && c.Description.ToLower().Contains(search)));
        }

        if (query.CategoryId.HasValue)
            courses = courses.Where(c => c.CategoryId == query.CategoryId.Value);

        if (query.Level.HasValue)
            courses = courses.Where(c => c.Level == query.Level.Value);

        if (query.MinPrice.HasValue)
            courses = courses.Where(c => c.Price >= query.MinPrice.Value);

        if (query.MaxPrice.HasValue)
            courses = courses.Where(c => c.Price <= query.MaxPrice.Value);

        if (query.IsPublished.HasValue)
            courses = courses.Where(c => c.IsPublished == query.IsPublished.Value);

        courses = query.SortBy?.ToLower() switch
        {
            "price" => query.SortDescending ? courses.OrderByDescending(c => c.Price) : courses.OrderBy(c => c.Price),
            "rating" => query.SortDescending ? courses.OrderByDescending(c => c.AverageRating) : courses.OrderBy(c => c.AverageRating),
            _ => courses.OrderBy(c => c.Title)
        };

        var result = courses
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToList();

        return Task.FromResult(Result<List<CourseDto>>.Success(result));
    }

    public Task<Result<CourseDto>> GetByIdAsync(Guid Id)
    {
        var course = Courses.FirstOrDefault(c => c.Id == Id);

        if (course is null)
            return Task.FromResult(Result<CourseDto>.Failure("Course not found.", ErrorType.NotFound));

        return Task.FromResult(Result<CourseDto>.Success(course));
    }

    public Task<Result<CourseDto>> CreateAsync(string instructorId, CreateCourseDto dto)
    {
        if (string.IsNullOrWhiteSpace(instructorId))
            return Task.FromResult(Result<CourseDto>.Failure("User is not authorized.", ErrorType.Unauthorized));

        if (string.IsNullOrWhiteSpace(dto.Title))
            return Task.FromResult(Result<CourseDto>.Failure("Course title is required.", ErrorType.Validation));

        if (dto.Title.Length > 200)
            return Task.FromResult(Result<CourseDto>.Failure("Course title max length is 200 characters.", ErrorType.Validation));

        if (dto.Price < 0)
            return Task.FromResult(Result<CourseDto>.Failure("Course price cannot be negative.", ErrorType.Validation));

        var course = new CourseDto
        {
            Id = Guid.NewGuid(),
            Title = dto.Title.Trim(),
            Description = dto.Description,
            Price = dto.Price,
            Level = dto.Level,
            IsPublished = false,
            InstructorId = instructorId,
            InstructorName = "",
            CategoryId = dto.CategoryId,
            CategoryName = "",
            AverageRating = 0,
            EnrollmentsCount = 0
        };

        Courses.Add(course);
        _logger.LogInformation("Course {CourseId} created by {InstructorId}", course.Id, instructorId);

        return Task.FromResult(Result<CourseDto>.Success(course));
    }

    public Task<Result<CourseDto>> UpdateAsync(Guid Id, string userId, bool isAdmin, UpdateCourseDto dto)
    {
        var course = Courses.FirstOrDefault(c => c.Id == Id);

        if (course is null)
            return Task.FromResult(Result<CourseDto>.Failure("Course not found.", ErrorType.NotFound));

        if (!isAdmin && course.InstructorId != userId)
            return Task.FromResult(Result<CourseDto>.Failure("You can update only your own courses.", ErrorType.Forbidden));

        if (string.IsNullOrWhiteSpace(dto.Title))
            return Task.FromResult(Result<CourseDto>.Failure("Course title is required.", ErrorType.Validation));

        if (dto.Title.Length > 200)
            return Task.FromResult(Result<CourseDto>.Failure("Course title max length is 200 characters.", ErrorType.Validation));

        if (dto.Price < 0)
            return Task.FromResult(Result<CourseDto>.Failure("Course price cannot be negative.", ErrorType.Validation));

        course.Title = dto.Title.Trim();
        course.Description = dto.Description;
        course.Price = dto.Price;
        course.Level = dto.Level;
        course.CategoryId = dto.CategoryId;

        return Task.FromResult(Result<CourseDto>.Success(course));
    }

    public Task<Result> DeleteAsync(Guid Id, string userId, bool isAdmin)
    {
        var course = Courses.FirstOrDefault(c => c.Id == Id);

        if (course is null)
            return Task.FromResult(Result.Failure("Course not found.", ErrorType.NotFound));

        if (!isAdmin && course.InstructorId != userId)
            return Task.FromResult(Result.Failure("You can delete only your own courses.", ErrorType.Forbidden));

        Courses.Remove(course);

        return Task.FromResult(Result.Success("Course deleted successfully."));
    }

    public Task<Result<CourseDto>> PublishAsync(Guid Id, string userId, bool isAdmin)
    {
        var course = Courses.FirstOrDefault(c => c.Id == Id);

        if (course is null)
            return Task.FromResult(Result<CourseDto>.Failure("Course not found.", ErrorType.NotFound));

        if (!isAdmin && course.InstructorId != userId)
            return Task.FromResult(Result<CourseDto>.Failure("You can publish only your own courses.", ErrorType.Forbidden));

        course.IsPublished = !course.IsPublished;

        return Task.FromResult(Result<CourseDto>.Success(course));
    }

    public async Task<Result<CourseDto>> UploadThumbnailAsync(Guid Id, string userId, bool isAdmin, IFormFile file)
    {
        var course = Courses.FirstOrDefault(c => c.Id == Id);

        if (course is null)
            return Result<CourseDto>.Failure("Course not found.", ErrorType.NotFound);

        if (!isAdmin && course.InstructorId != userId)
            return Result<CourseDto>.Failure("You can update only your own courses.", ErrorType.Forbidden);

        if (file is null)
            return Result<CourseDto>.Failure("Thumbnail file is required.", ErrorType.Validation);

        if (file.Length <= 0 || file.Length > 5 * 1024 * 1024)
            return Result<CourseDto>.Failure("Thumbnail max size is 5MB.", ErrorType.Validation);

        if (file.ContentType != "image/jpeg" && file.ContentType != "image/png")
            return Result<CourseDto>.Failure("Thumbnail must be jpeg or png.", ErrorType.Validation);

        course.ThumbnailUrl = await _fileService.SaveCourseThumbnailAsync(course.Id, file);

        return Result<CourseDto>.Success(course);
    }
}
