using Application.DTOs.Courses;
using Domain.Entities;

namespace Application.Interfaces.Repositories;

public interface ICourseRepository
{
    Task<List<Course>> GetAsync(CourseQueryDto query);
    Task<Course?> GetByIdAsync(Guid id);
    Task<Course?> GetByIdForUpdateAsync(Guid id);
    Task<Category?> GetCategoryAsync(Guid id);
    Task<Course> CreateAsync(Course course);
    Task UpdateAsync(Course course);
    Task DeleteAsync(Course course);
}
