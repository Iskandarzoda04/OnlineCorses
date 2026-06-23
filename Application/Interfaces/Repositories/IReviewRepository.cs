using Domain.Entities;

namespace Application.Interfaces.Repositories;

public interface IReviewRepository
{
    Task<bool> CourseExistsAsync(Guid courseId);
    Task<bool> IsStudentEnrolledAsync(string studentId, Guid courseId);
    Task<bool> ExistsAsync(string studentId, Guid courseId);
    Task<List<Review>> GetByCourseAsync(Guid courseId);
    Task<Review?> GetByIdAsync(Guid id);
    Task<Review?> GetByCourseAndIdAsync(Guid courseId, Guid id);
    Task<Review> CreateAsync(Review review);
    Task UpdateAsync(Review review);
    Task DeleteAsync(Review review);
}
