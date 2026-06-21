using Application.Common;
using Application.DTOs.Enrollments;

namespace Application.Interfaces.Services;

public interface IEnrollmentService
{
    Task<Result<EnrollmentDto>> EnrollAsync(string studentId, Guid CourseId);
    Task<Result<IReadOnlyList<EnrollmentDto>>> GetMineAsync(string studentId);
    Task<Result<IReadOnlyList<EnrollmentDto>>> GetAllAsync();
    Task<Result<EnrollmentDto>> UpdateProgressAsync(string studentId, Guid enrollmentId, UpdateEnrollmentProgressDto dto);
    Task<Result> CancelAsync(string studentId, Guid enrollmentId);
}
