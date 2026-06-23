using Domain.Entities;

namespace Application.Interfaces.Repositories;

public interface ILessonRepository
{
    Task<List<Lesson>> GetByCourseAsync(Guid courseId);
    Task<Lesson?> GetByIdAsync(Guid id);
    Task<Lesson?> GetByCourseAndIdAsync(Guid courseId, Guid id);
    Task<Course?> GetCourseAsync(Guid courseId);
    Task<Lesson> CreateAsync(Lesson lesson);
    Task UpdateAsync(Lesson lesson);
    Task DeleteAsync(Lesson lesson);
}
