using Application.Common;
using Application.DTOs.Enrollments;
using Application.Interfaces.Services;
using Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class EnrollmentService : IEnrollmentService
{
    private static readonly List<EnrollmentDto> Enrollments = new();
    private readonly ILogger<EnrollmentService> _logger;

    public EnrollmentService(ILogger<EnrollmentService> logger)
    {
        _logger = logger;
    }

    public Task<Result<EnrollmentDto>> EnrollAsync(string studentId, Guid CourseId)
    {
        if (string.IsNullOrWhiteSpace(studentId))
            return Task.FromResult(Result<EnrollmentDto>.Failure("StudentId is required.", ErrorType.Validation));

        if (CourseId == Guid.Empty)
            return Task.FromResult(Result<EnrollmentDto>.Failure("CourseId is required.", ErrorType.Validation));

        var exists = Enrollments.Any(e =>
            e.StudentId == studentId &&
            e.CourseId == CourseId &&
            e.Status != EnrollmentStatus.Cancelled);

        if (exists)
            return Task.FromResult(Result<EnrollmentDto>.Failure("Student cannot enroll twice in same course.", ErrorType.Validation));

        var enrollment = new EnrollmentDto
        {
            Id = Guid.NewGuid(),
            StudentId = studentId,
            CourseId = CourseId,
            CourseTitle = "",
            Status = EnrollmentStatus.Active,
            ProgressPercent = 0,
            EnrolledAt = DateTime.UtcNow
        };

        Enrollments.Add(enrollment);
        _logger.LogInformation("Student {StudentId} enrolled to course {CourseId}", studentId, CourseId);

        return Task.FromResult(Result<EnrollmentDto>.Success(enrollment));
    }

    public Task<Result<IReadOnlyList<EnrollmentDto>>> GetMineAsync(string studentId)
    {
        if (string.IsNullOrWhiteSpace(studentId))
            return Task.FromResult(Result<IReadOnlyList<EnrollmentDto>>.Failure("StudentId is required.", ErrorType.Validation));

        IReadOnlyList<EnrollmentDto> enrollments = Enrollments
            .Where(e => e.StudentId == studentId)
            .ToList();

        return Task.FromResult(Result<IReadOnlyList<EnrollmentDto>>.Success(enrollments));
    }

    public Task<Result<IReadOnlyList<EnrollmentDto>>> GetAllAsync()
    {
        IReadOnlyList<EnrollmentDto> enrollments = Enrollments.ToList();
        return Task.FromResult(Result<IReadOnlyList<EnrollmentDto>>.Success(enrollments));
    }

    public Task<Result<EnrollmentDto>> UpdateProgressAsync(string studentId, Guid enrollmentId, UpdateEnrollmentProgressDto dto)
    {
        if (string.IsNullOrWhiteSpace(studentId))
            return Task.FromResult(Result<EnrollmentDto>.Failure("StudentId is required.", ErrorType.Validation));

        if (dto.ProgressPercent < 0 || dto.ProgressPercent > 100)
            return Task.FromResult(Result<EnrollmentDto>.Failure("ProgressPercent must be between 0 and 100.", ErrorType.Validation));

        var enrollment = Enrollments.FirstOrDefault(e => e.Id == enrollmentId);

        if (enrollment is null)
            return Task.FromResult(Result<EnrollmentDto>.Failure("Enrollment not found.", ErrorType.NotFound));

        if (enrollment.StudentId != studentId)
            return Task.FromResult(Result<EnrollmentDto>.Failure("You can update only your own enrollment.", ErrorType.Forbidden));

        if (enrollment.Status == EnrollmentStatus.Cancelled)
            return Task.FromResult(Result<EnrollmentDto>.Failure("Cancelled enrollment cannot be updated.", ErrorType.Validation));

        enrollment.ProgressPercent = dto.ProgressPercent;
        enrollment.Status = dto.ProgressPercent == 100 ? EnrollmentStatus.Completed : EnrollmentStatus.Active;

        return Task.FromResult(Result<EnrollmentDto>.Success(enrollment));
    }

    public Task<Result> CancelAsync(string studentId, Guid enrollmentId)
    {
        if (string.IsNullOrWhiteSpace(studentId))
            return Task.FromResult(Result.Failure("StudentId is required.", ErrorType.Validation));

        var enrollment = Enrollments.FirstOrDefault(e => e.Id == enrollmentId);

        if (enrollment is null)
            return Task.FromResult(Result.Failure("Enrollment not found.", ErrorType.NotFound));

        if (enrollment.StudentId != studentId)
            return Task.FromResult(Result.Failure("You can cancel only your own enrollment.", ErrorType.Forbidden));

        enrollment.Status = EnrollmentStatus.Cancelled;

        return Task.FromResult(Result.Success("Enrollment cancelled successfully."));
    }
}
