using Application.Interfaces.Repositories;
using Application.Results;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class EnrollmentRepository(AppDbContext context) : IEnrollmentRepository
{
    public async Task<Course?> GetCourseAsync(Guid courseId)
    {
        return await context.Courses.FirstOrDefaultAsync(c => c.Id == courseId);
    }

    public async Task<bool> ExistsAsync(string studentId, Guid courseId)
    {
        return await context.Enrollments.AnyAsync(e => e.StudentId == studentId && e.CourseId == courseId);
    }

    public async Task<Enrollment?> GetByIdAsync(Guid id)
    {
        return await context.Enrollments
            .Include(e => e.Course)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<Enrollment?> GetByStudentAndCourseAsync(string studentId, Guid courseId)
    {
        return await context.Enrollments
            .FirstOrDefaultAsync(e => e.StudentId == studentId && e.CourseId == courseId);
    }

    public async Task<List<Enrollment>> GetByStudentIdAsync(string studentId)
    {
        return await context.Enrollments
            .Include(e => e.Course)
            .Where(e => e.StudentId == studentId)
            .ToListAsync();
    }

    public async Task<PagedResult<Enrollment>> GetAllAsync(int page, int pageSize)
    {
        var query = context.Enrollments
            .Include(e => e.Course)
            .AsQueryable();

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return PagedResult<Enrollment>.Ok(items, totalCount, page, pageSize);
    }

    public async Task<Enrollment> CreateAsync(Enrollment enrollment)
    {
        context.Enrollments.Add(enrollment);
        await context.SaveChangesAsync();
        return enrollment;
    }

    public async Task UpdateAsync(Enrollment enrollment)
    {
        context.Enrollments.Update(enrollment);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Enrollment enrollment)
    {
        context.Enrollments.Remove(enrollment);
        await context.SaveChangesAsync();
    }
}
