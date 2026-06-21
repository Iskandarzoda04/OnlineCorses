using Application.Common;
using Application.DTOs.Lessons;

namespace Application.Interfaces.Services;

public interface ILessonService
{
    Task<Result<IReadOnlyList<LessonDto>>> GetByCourseAsync(Guid CourseId);
    Task<Result<LessonDto>> GetByIdAsync(Guid courseId, Guid id);
    Task<Result<LessonDto>> CreateAsync(string userId, bool isAdmin, CreateLessonDto dto);
    Task<Result<LessonDto>> UpdateAsync(Guid Id, string userId, bool isAdmin, UpdateLessonDto dto);
    Task<Result> DeleteAsync(Guid Id, string userId, bool isAdmin);
}
