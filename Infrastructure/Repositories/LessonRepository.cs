using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class LessonRepository(AppDbContext context) : ILessonRepository
{
    public Task<List<Lesson>> GetByCourseAsync(Guid courseId)
    {
        return context.Lessons
            .AsNoTracking()
            .Where(l => l.CourseId == courseId)
            .OrderBy(l => l.Order)
            .ToListAsync();
    }

    public Task<Lesson?> GetByIdAsync(Guid id)
    {
        return context.Lessons
            .Include(l => l.Course)
            .FirstOrDefaultAsync(l => l.Id == id);
    }

    public Task<Lesson?> GetByCourseAndIdAsync(Guid courseId, Guid id)
    {
        return context.Lessons
            .Include(l => l.Course)
            .FirstOrDefaultAsync(l => l.CourseId == courseId && l.Id == id);
    }

    public Task<Course?> GetCourseAsync(Guid courseId)
    {
        return context.Courses.FirstOrDefaultAsync(c => c.Id == courseId);
    }

    public async Task<Lesson> CreateAsync(Lesson lesson)
    {
        context.Lessons.Add(lesson);
        await context.SaveChangesAsync();
        return lesson;
    }

    public async Task UpdateAsync(Lesson lesson)
    {
        context.Lessons.Update(lesson);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Lesson lesson)
    {
        context.Lessons.Remove(lesson);
        await context.SaveChangesAsync();
    }
}
