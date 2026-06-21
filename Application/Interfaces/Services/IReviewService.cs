using Application.Common;
using Application.DTOs.Reviews;

namespace Application.Interfaces.Services;

public interface IReviewService
{
    Task<Result<IReadOnlyList<ReviewDto>>> GetByCourseAsync(Guid CourseId);
    Task<Result<ReviewDto>> CreateAsync(string studentId, CreateReviewDto dto);
    Task<Result<ReviewDto>> UpdateAsync(Guid Id, string studentId, UpdateReviewDto dto);
    Task<Result> DeleteAsync(Guid Id, string userId, bool isAdmin);
}
