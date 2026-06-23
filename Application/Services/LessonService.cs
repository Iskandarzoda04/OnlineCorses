using Application.Common;
using Application.DTOs.Lessons;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class LessonService : ILessonService
{
    private readonly ILessonRepository _lessonRepository;
    private readonly ILogger<LessonService> _logger;

    public LessonService(
        ILessonRepository lessonRepository,
        ILogger<LessonService> logger)
    {
        _lessonRepository = lessonRepository;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<LessonDto>>> GetByCourseAsync(Guid CourseId)
    {
        if (CourseId == Guid.Empty)
            return Result<IReadOnlyList<LessonDto>>.Failure("CourseId is required.", ErrorType.Validation);

        var course = await _lessonRepository.GetCourseAsync(CourseId);
        if (course is null)
            return Result<IReadOnlyList<LessonDto>>.Failure("Course not found.", ErrorType.NotFound);

        var lessons = await _lessonRepository.GetByCourseAsync(CourseId);
        return Result<IReadOnlyList<LessonDto>>.Success(lessons.Select(MapToDto).ToList());
    }

    public async Task<Result<LessonDto>> GetByIdAsync(Guid courseId, Guid id)
    {
        var lesson = await _lessonRepository.GetByCourseAndIdAsync(courseId, id);

        if (lesson is null)
            return Result<LessonDto>.Failure("Lesson not found.", ErrorType.NotFound);

        return Result<LessonDto>.Success(MapToDto(lesson));
    }

    public async Task<Result<LessonDto>> CreateAsync(string userId, bool isAdmin, CreateLessonDto dto)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return Result<LessonDto>.Failure("User is not authorized.", ErrorType.Unauthorized);

        if (dto.CourseId == Guid.Empty)
            return Result<LessonDto>.Failure("CourseId is required.", ErrorType.Validation);

        if (string.IsNullOrWhiteSpace(dto.Title))
            return Result<LessonDto>.Failure("Lesson title is required.", ErrorType.Validation);

        if (dto.Title.Length > 200)
            return Result<LessonDto>.Failure("Lesson title max length is 200 characters.", ErrorType.Validation);

        if (dto.Order < 0)
            return Result<LessonDto>.Failure("Lesson order cannot be negative.", ErrorType.Validation);

        if (dto.DurationMinutes < 0)
            return Result<LessonDto>.Failure("DurationMinutes cannot be negative.", ErrorType.Validation);

        var course = await _lessonRepository.GetCourseAsync(dto.CourseId);
        if (course is null)
            return Result<LessonDto>.Failure("Course not found.", ErrorType.NotFound);

        if (!isAdmin && course.InstructorId != userId)
            return Result<LessonDto>.Failure("You can add lessons only to your own courses.", ErrorType.Forbidden);

        var lesson = new Lesson
        {
            CourseId = dto.CourseId,
            Title = dto.Title.Trim(),
            Content = dto.Content,
            VideoUrl = dto.VideoUrl,
            Order = dto.Order,
            DurationMinutes = dto.DurationMinutes
        };

        await _lessonRepository.CreateAsync(lesson);

        _logger.LogInformation("Lesson {LessonId} created by {UserId}", lesson.Id, userId);

        return Result<LessonDto>.Success(MapToDto(lesson));
    }

    public async Task<Result<LessonDto>> UpdateAsync(Guid courseId, Guid Id, string userId, bool isAdmin, UpdateLessonDto dto)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return Result<LessonDto>.Failure("User is not authorized.", ErrorType.Unauthorized);

        var lesson = await _lessonRepository.GetByCourseAndIdAsync(courseId, Id);

        if (lesson is null)
            return Result<LessonDto>.Failure("Lesson not found.", ErrorType.NotFound);

        if (!isAdmin && lesson.Course.InstructorId != userId)
            return Result<LessonDto>.Failure("You can update lessons only in your own courses.", ErrorType.Forbidden);

        if (string.IsNullOrWhiteSpace(dto.Title))
            return Result<LessonDto>.Failure("Lesson title is required.", ErrorType.Validation);

        if (dto.Title.Length > 200)
            return Result<LessonDto>.Failure("Lesson title max length is 200 characters.", ErrorType.Validation);

        if (dto.Order < 0)
            return Result<LessonDto>.Failure("Lesson order cannot be negative.", ErrorType.Validation);

        if (dto.DurationMinutes < 0)
            return Result<LessonDto>.Failure("DurationMinutes cannot be negative.", ErrorType.Validation);

        lesson.Title = dto.Title.Trim();
        lesson.Content = dto.Content;
        lesson.VideoUrl = dto.VideoUrl;
        lesson.Order = dto.Order;
        lesson.DurationMinutes = dto.DurationMinutes;

        await _lessonRepository.UpdateAsync(lesson);

        return Result<LessonDto>.Success(MapToDto(lesson));
    }

    public async Task<Result> DeleteAsync(Guid courseId, Guid Id, string userId, bool isAdmin)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return Result.Failure("User is not authorized.", ErrorType.Unauthorized);

        var lesson = await _lessonRepository.GetByCourseAndIdAsync(courseId, Id);

        if (lesson is null)
            return Result.Failure("Lesson not found.", ErrorType.NotFound);

        if (!isAdmin && lesson.Course.InstructorId != userId)
            return Result.Failure("You can delete lessons only in your own courses.", ErrorType.Forbidden);

        await _lessonRepository.DeleteAsync(lesson);

        return Result.Success("Lesson deleted successfully.");
    }

    private static LessonDto MapToDto(Lesson lesson)
    {
        return new LessonDto
        {
            Id = lesson.Id,
            CourseId = lesson.CourseId,
            Title = lesson.Title,
            Content = lesson.Content,
            VideoUrl = lesson.VideoUrl,
            Order = lesson.Order,
            DurationMinutes = lesson.DurationMinutes
        };
    }
}
