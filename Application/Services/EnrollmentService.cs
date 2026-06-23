using Application.Common;
using Application.DTOs.Enrollments;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class EnrollmentService : IEnrollmentService
{
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly ICacheService _cache;
    private readonly ILogger<EnrollmentService> _logger;

    public EnrollmentService(
        IEnrollmentRepository enrollmentRepository,
        ICacheService cache,
        ILogger<EnrollmentService> logger)
    {
        _enrollmentRepository = enrollmentRepository;
        _cache = cache;
        _logger = logger;
    }

    public async Task<Result<EnrollmentDto>> EnrollAsync(string studentId, Guid CourseId)
    {
        if (string.IsNullOrWhiteSpace(studentId))
            return Result<EnrollmentDto>.Failure("StudentId is required.", ErrorType.Validation);

        if (CourseId == Guid.Empty)
            return Result<EnrollmentDto>.Failure("CourseId is required.", ErrorType.Validation);

        var course = await _enrollmentRepository.GetCourseAsync(CourseId);
        if (course is null)
            return Result<EnrollmentDto>.Failure("Course not found.", ErrorType.NotFound);

        var exists = await _enrollmentRepository.GetByStudentAndCourseAsync(studentId, CourseId);
        if (exists is not null)
            return Result<EnrollmentDto>.Failure("Student cannot enroll twice in same course.", ErrorType.Validation);

        var enrollment = new Enrollment
        {
            StudentId = studentId,
            CourseId = CourseId,
            Status = EnrollmentStatus.Active,
            ProgressPercent = 0
        };

        await _enrollmentRepository.CreateAsync(enrollment);
        await _cache.RemoveAsync("dashboard:summary");
        await _cache.RemoveAsync("dashboard:top-courses");

        _logger.LogInformation("Student {StudentId} enrolled to course {CourseId}", studentId, CourseId);

        return Result<EnrollmentDto>.Success(MapToDto(enrollment));
    }

    public async Task<Result<IReadOnlyList<EnrollmentDto>>> GetMineAsync(string studentId)
    {
        if (string.IsNullOrWhiteSpace(studentId))
            return Result<IReadOnlyList<EnrollmentDto>>.Failure("StudentId is required.", ErrorType.Validation);

        var enrollments = await _enrollmentRepository.GetByStudentIdAsync(studentId);
        return Result<IReadOnlyList<EnrollmentDto>>.Success(enrollments.Select(MapToDto).ToList());
    }

    public async Task<Result<IReadOnlyList<EnrollmentDto>>> GetAllAsync()
    {
        var enrollments = await _enrollmentRepository.GetAllAsync(1, 50);
        return Result<IReadOnlyList<EnrollmentDto>>.Success(enrollments.Items.Select(MapToDto).ToList());
    }

    public async Task<Result<EnrollmentDto>> UpdateProgressAsync(string studentId, Guid enrollmentId, UpdateEnrollmentProgressDto dto)
    {
        if (string.IsNullOrWhiteSpace(studentId))
            return Result<EnrollmentDto>.Failure("StudentId is required.", ErrorType.Validation);

        if (dto.ProgressPercent < 0 || dto.ProgressPercent > 100)
            return Result<EnrollmentDto>.Failure("ProgressPercent must be between 0 and 100.", ErrorType.Validation);

        var enrollment = await _enrollmentRepository.GetByIdAsync(enrollmentId);

        if (enrollment is null)
            return Result<EnrollmentDto>.Failure("Enrollment not found.", ErrorType.NotFound);

        if (enrollment.StudentId != studentId)
            return Result<EnrollmentDto>.Failure("You can update only your own enrollment.", ErrorType.Forbidden);

        if (enrollment.Status == EnrollmentStatus.Cancelled)
            return Result<EnrollmentDto>.Failure("Cancelled enrollment cannot be updated.", ErrorType.Validation);

        enrollment.ProgressPercent = dto.ProgressPercent;
        enrollment.Status = dto.ProgressPercent == 100 ? EnrollmentStatus.Completed : EnrollmentStatus.Active;
        enrollment.CompletedAt = dto.ProgressPercent == 100 ? DateTime.UtcNow : null;

        await _enrollmentRepository.UpdateAsync(enrollment);
        await _cache.RemoveAsync("dashboard:summary");
        await _cache.RemoveAsync("dashboard:top-courses");

        return Result<EnrollmentDto>.Success(MapToDto(enrollment));
    }

    public async Task<Result> CancelAsync(string studentId, Guid enrollmentId)
    {
        if (string.IsNullOrWhiteSpace(studentId))
            return Result.Failure("StudentId is required.", ErrorType.Validation);

        var enrollment = await _enrollmentRepository.GetByIdAsync(enrollmentId);

        if (enrollment is null)
            return Result.Failure("Enrollment not found.", ErrorType.NotFound);

        if (enrollment.StudentId != studentId)
            return Result.Failure("You can cancel only your own enrollment.", ErrorType.Forbidden);

        enrollment.Status = EnrollmentStatus.Cancelled;

        await _enrollmentRepository.UpdateAsync(enrollment);
        await _cache.RemoveAsync("dashboard:summary");
        await _cache.RemoveAsync("dashboard:top-courses");

        return Result.Success("Enrollment cancelled successfully.");
    }

    private static EnrollmentDto MapToDto(Enrollment enrollment)
    {
        return new EnrollmentDto
        {
            Id = enrollment.Id,
            StudentId = enrollment.StudentId,
            CourseId = enrollment.CourseId,
            CourseTitle = enrollment.Course?.Title ?? "",
            Status = enrollment.Status,
            ProgressPercent = enrollment.ProgressPercent,
            EnrolledAt = enrollment.EnrolledAt
        };
    }
}
