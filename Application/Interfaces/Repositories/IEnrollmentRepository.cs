using Domain.Entities;
using Application.Results;

namespace Application.Interfaces.Repositories;

public interface IEnrollmentRepository
{
    Task<Course?> GetCourseAsync(Guid courseId);
    Task<bool> ExistsAsync(string studentId, Guid courseId);
    Task<Enrollment?> GetByStudentAndCourseAsync(string studentId, Guid courseId);
    Task<Enrollment> CreateAsync(Enrollment enrollment);
    Task<List<Enrollment>> GetByStudentIdAsync(string studentId);
    Task<PagedResult<Enrollment>> GetAllAsync(int page, int pageSize);
    Task<Enrollment?> GetByIdAsync(Guid id);
    Task UpdateAsync(Enrollment enrollment);
    Task DeleteAsync(Enrollment enrollment);
}
