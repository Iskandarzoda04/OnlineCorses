using Application.Interfaces.Repositories;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ReviewRepository(AppDbContext context) : IReviewRepository
{
    public Task<bool> CourseExistsAsync(Guid courseId)
    {
        return context.Courses.AnyAsync(c => c.Id == courseId);
    }

    public Task<bool> IsStudentEnrolledAsync(string studentId, Guid courseId)
    {
        return context.Enrollments.AnyAsync(e =>
            e.StudentId == studentId &&
            e.CourseId == courseId &&
            e.Status != EnrollmentStatus.Cancelled);
    }

    public Task<bool> ExistsAsync(string studentId, Guid courseId)
    {
        return context.Reviews.AnyAsync(r => r.StudentId == studentId && r.CourseId == courseId);
    }

    public Task<List<Review>> GetByCourseAsync(Guid courseId)
    {
        return context.Reviews
            .AsNoTracking()
            .Include(r => r.Student)
            .Where(r => r.CourseId == courseId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public Task<Review?> GetByIdAsync(Guid id)
    {
        return context.Reviews
            .Include(r => r.Student)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public Task<Review?> GetByCourseAndIdAsync(Guid courseId, Guid id)
    {
        return context.Reviews
            .Include(r => r.Student)
            .FirstOrDefaultAsync(r => r.CourseId == courseId && r.Id == id);
    }

    public async Task<Review> CreateAsync(Review review)
    {
        context.Reviews.Add(review);
        await context.SaveChangesAsync();
        return review;
    }

    public async Task UpdateAsync(Review review)
    {
        context.Reviews.Update(review);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Review review)
    {
        context.Reviews.Remove(review);
        await context.SaveChangesAsync();
    }
}
