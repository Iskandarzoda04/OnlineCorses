using Application.DTOs.Courses;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class CourseRepository(AppDbContext context) : ICourseRepository
{
    public async Task<List<Course>> GetAsync(CourseQueryDto query)
    {
        var courses = context.Courses
            .Include(c => c.Instructor)
            .Include(c => c.Category)
            .Include(c => c.Lessons)
            .Include(c => c.Enrollments)
            .Include(c => c.Reviews)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
            courses = courses.Where(c => c.Description != null && c.Title != null &&
                (c.Title.Contains(query.Search) || c.Description.Contains(query.Search)));

        if (query.CategoryId.HasValue)
            courses = courses.Where(c => c.CategoryId == query.CategoryId);

        if (query.Level.HasValue)
            courses = courses.Where(c => c.Level == query.Level);

        if (query.MinPrice.HasValue)
            courses = courses.Where(c => c.Price >= query.MinPrice);

        if (query.MaxPrice.HasValue)
            courses = courses.Where(c => c.Price <= query.MaxPrice);

        if (query.IsPublished.HasValue)
            courses = courses.Where(c => c.IsPublished == query.IsPublished);

        return await courses
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();
    }

    public async Task<Course?> GetByIdAsync(Guid id)
    {
        return await context.Courses
            .Include(c => c.Instructor)
            .Include(c => c.Category)
            .Include(c => c.Lessons)
            .Include(c => c.Enrollments)
            .Include(c => c.Reviews)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Course?> GetByIdForUpdateAsync(Guid id)
    {
        return await context.Courses
            .Include(c => c.Instructor)
            .Include(c => c.Category)
            .Include(c => c.Lessons)
            .Include(c => c.Enrollments)
            .Include(c => c.Reviews)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Category?> GetCategoryAsync(Guid id)
    {
        return await context.Categories.FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Course> CreateAsync(Course course)
    {
        context.Courses.Add(course);
        await context.SaveChangesAsync();
        return course;
    }

    public async Task UpdateAsync(Course course)
    {
        context.Courses.Update(course);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Course course)
    {
        context.Courses.Remove(course);
        await context.SaveChangesAsync();
    }

    public async Task<List<Course>> GetAllAsync()
    {
        return await context.Courses
            .Include(c => c.Instructor)
            .Include(c => c.Category)
            .Include(c => c.Lessons)
            .Include(c => c.Enrollments)
            .Include(c => c.Reviews)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

}
