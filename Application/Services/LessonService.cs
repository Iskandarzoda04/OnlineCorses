using Application.Common;
using Application.DTOs.Lessons;
using Application.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class LessonService : ILessonService
{
    private static readonly List<LessonDto> Lessons = new();
    private readonly ILogger<LessonService> _logger;

    public LessonService(ILogger<LessonService> logger)
    {
        _logger = logger;
    }

    public Task<Result<IReadOnlyList<LessonDto>>> GetByCourseAsync(Guid CourseId)
    {
        if (CourseId == Guid.Empty)
            return Task.FromResult(Result<IReadOnlyList<LessonDto>>.Failure("CourseId is required.", ErrorType.Validation));

        IReadOnlyList<LessonDto> lessons = Lessons
            .Where(l => l.CourseId == CourseId)
            .OrderBy(l => l.Order)
            .ToList();

        return Task.FromResult(Result<IReadOnlyList<LessonDto>>.Success(lessons));
    }

    public Task<Result<LessonDto>> GetByIdAsync(Guid courseId, Guid id)
    {
        var lesson = Lessons.FirstOrDefault(l => l.CourseId == courseId && l.Id == id);

        if (lesson is null)
            return Task.FromResult(Result<LessonDto>.Failure("Lesson not found.", ErrorType.NotFound));

        return Task.FromResult(Result<LessonDto>.Success(lesson));
    }

    public Task<Result<LessonDto>> CreateAsync(string userId, bool isAdmin, CreateLessonDto dto)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return Task.FromResult(Result<LessonDto>.Failure("User is not authorized.", ErrorType.Unauthorized));

        if (dto.CourseId == Guid.Empty)
            return Task.FromResult(Result<LessonDto>.Failure("CourseId is required.", ErrorType.Validation));

        if (string.IsNullOrWhiteSpace(dto.Title))
            return Task.FromResult(Result<LessonDto>.Failure("Lesson title is required.", ErrorType.Validation));

        if (dto.Order < 0)
            return Task.FromResult(Result<LessonDto>.Failure("Lesson order cannot be negative.", ErrorType.Validation));

        if (dto.DurationMinutes < 0)
            return Task.FromResult(Result<LessonDto>.Failure("DurationMinutes cannot be negative.", ErrorType.Validation));

        var lesson = new LessonDto
        {
            Id = Guid.NewGuid(),
            CourseId = dto.CourseId,
            Title = dto.Title.Trim(),
            Content = dto.Content,
            VideoUrl = dto.VideoUrl,
            Order = dto.Order,
            DurationMinutes = dto.DurationMinutes
        };

        Lessons.Add(lesson);
        _logger.LogInformation("Lesson {LessonId} created by {UserId}", lesson.Id, userId);

        return Task.FromResult(Result<LessonDto>.Success(lesson));
    }

    public Task<Result<LessonDto>> UpdateAsync(Guid Id, string userId, bool isAdmin, UpdateLessonDto dto)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return Task.FromResult(Result<LessonDto>.Failure("User is not authorized.", ErrorType.Unauthorized));

        var lesson = Lessons.FirstOrDefault(l => l.Id == Id);

        if (lesson is null)
            return Task.FromResult(Result<LessonDto>.Failure("Lesson not found.", ErrorType.NotFound));

        if (string.IsNullOrWhiteSpace(dto.Title))
            return Task.FromResult(Result<LessonDto>.Failure("Lesson title is required.", ErrorType.Validation));

        if (dto.Order < 0)
            return Task.FromResult(Result<LessonDto>.Failure("Lesson order cannot be negative.", ErrorType.Validation));

        if (dto.DurationMinutes < 0)
            return Task.FromResult(Result<LessonDto>.Failure("DurationMinutes cannot be negative.", ErrorType.Validation));

        lesson.Title = dto.Title.Trim();
        lesson.Content = dto.Content;
        lesson.VideoUrl = dto.VideoUrl;
        lesson.Order = dto.Order;
        lesson.DurationMinutes = dto.DurationMinutes;

        return Task.FromResult(Result<LessonDto>.Success(lesson));
    }

    public Task<Result> DeleteAsync(Guid Id, string userId, bool isAdmin)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return Task.FromResult(Result.Failure("User is not authorized.", ErrorType.Unauthorized));

        var lesson = Lessons.FirstOrDefault(l => l.Id == Id);

        if (lesson is null)
            return Task.FromResult(Result.Failure("Lesson not found.", ErrorType.NotFound));

        Lessons.Remove(lesson);

        return Task.FromResult(Result.Success("Lesson deleted successfully."));
    }
}
