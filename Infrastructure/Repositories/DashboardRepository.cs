using Application.DTOs.Dashboard;
using Application.Interfaces.Repositories;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class DashboardRepository(AppDbContext context) : IDashboardRepository
{
    public Task<int> GetTotalCoursesAsync()
    {
        return context.Courses.CountAsync();
    }

    public Task<int> GetPublishedCoursesAsync()
    {
        return context.Courses.CountAsync(c => c.IsPublished);
    }

    public Task<int> GetTotalEnrollmentsAsync()
    {
        return context.Enrollments.CountAsync();
    }

    public Task<int> GetActiveEnrollmentsAsync()
    {
        return context.Enrollments.CountAsync(e => e.Status == EnrollmentStatus.Active);
    }

    public Task<int> GetCompletedEnrollmentsAsync()
    {
        return context.Enrollments.CountAsync(e => e.Status == EnrollmentStatus.Completed);
    }

    public async Task<decimal> GetTotalRevenueAsync()
    {
        return await context.Enrollments
            .Where(e => e.Status != EnrollmentStatus.Cancelled)
            .SumAsync(e => (decimal?)e.Course.Price) ?? 0;
    }

    public async Task<double> GetAverageRatingAsync()
    {
        var totalReviews = await context.Reviews.CountAsync();
        if (totalReviews == 0)
            return 0;

        return await context.Reviews.AverageAsync(r => r.Rating);
    }

    public Task<int> GetTotalReviewsAsync()
    {
        return context.Reviews.CountAsync();
    }

    public async Task<IReadOnlyList<TopCourseDto>> GetTopCoursesAsync()
    {
        var courses = await context.Courses
            .AsNoTracking()
            .Include(c => c.Instructor)
            .Include(c => c.Enrollments)
            .Include(c => c.Reviews)
            .ToListAsync();

        return courses
            .Select(c =>
            {
                var enrollmentCount = c.Enrollments.Count(e => e.Status != EnrollmentStatus.Cancelled);
                var completedCount = c.Enrollments.Count(e => e.Status == EnrollmentStatus.Completed);

                return new TopCourseDto
                {
                    CourseId = c.Id,
                    Title = c.Title,
                    InstructorName = c.Instructor.FullName,
                    EnrollmentCount = enrollmentCount,
                    CompletedCount = completedCount,
                    CompletionRate = enrollmentCount == 0 ? 0 : completedCount * 100.0 / enrollmentCount,
                    AverageRating = c.Reviews.Count == 0 ? 0 : c.Reviews.Average(r => r.Rating),
                    Revenue = enrollmentCount * c.Price
                };
            })
            .OrderByDescending(c => c.EnrollmentCount)
            .ThenByDescending(c => c.AverageRating)
            .Take(10)
            .ToList();
    }

    public async Task<IReadOnlyList<MonthlyEnrollmentDto>> GetEnrollmentsByMonthAsync()
    {
        var enrollments = await context.Enrollments
            .AsNoTracking()
            .Include(e => e.Course)
            .ToListAsync();

        return enrollments
            .GroupBy(e => new { e.EnrolledAt.Year, e.EnrolledAt.Month })
            .OrderBy(g => g.Key.Year)
            .ThenBy(g => g.Key.Month)
            .Select(g => new MonthlyEnrollmentDto
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                MonthName = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMMM"),
                NewEnrollments = g.Count(),
                Completions = g.Count(e => e.Status == EnrollmentStatus.Completed),
                Revenue = g.Where(e => e.Status != EnrollmentStatus.Cancelled).Sum(e => e.Course.Price)
            })
            .ToList();
    }

    public async Task<IReadOnlyList<CategoryRevenueDto>> GetRevenueByCategoryAsync()
    {
        var categories = await context.Categories
            .AsNoTracking()
            .Include(c => c.Courses)
            .ThenInclude(c => c.Enrollments)
            .Include(c => c.Courses)
            .ThenInclude(c => c.Reviews)
            .ToListAsync();

        return categories
            .Select(c =>
            {
                var enrollments = c.Courses
                    .SelectMany(course => course.Enrollments)
                    .Where(e => e.Status != EnrollmentStatus.Cancelled)
                    .ToList();

                var reviews = c.Courses.SelectMany(course => course.Reviews).ToList();

                return new CategoryRevenueDto
                {
                    CategoryId = c.Id,
                    CategoryName = c.Name,
                    CourseCount = c.Courses.Count,
                    TotalStudents = enrollments.Select(e => e.StudentId).Distinct().Count(),
                    TotalRevenue = enrollments.Sum(e => c.Courses.First(course => course.Id == e.CourseId).Price),
                    AverageRating = reviews.Count == 0 ? 0 : reviews.Average(r => r.Rating)
                };
            })
            .OrderByDescending(c => c.TotalRevenue)
            .ToList();
    }

    public async Task<IReadOnlyList<CompletionRateDto>> GetCompletionRateAsync()
    {
        var courses = await context.Courses
            .AsNoTracking()
            .Include(c => c.Enrollments)
            .ToListAsync();

        return courses
            .Select(c =>
            {
                var totalEnrolled = c.Enrollments.Count(e => e.Status != EnrollmentStatus.Cancelled);
                var totalCompleted = c.Enrollments.Count(e => e.Status == EnrollmentStatus.Completed);

                return new CompletionRateDto
                {
                    CourseId = c.Id,
                    Title = c.Title,
                    TotalEnrolled = totalEnrolled,
                    TotalCompleted = totalCompleted,
                    CompletionRatePercent = totalEnrolled == 0 ? 0 : totalCompleted * 100.0 / totalEnrolled,
                    AverageProgressPercent = totalEnrolled == 0 ? 0 : c.Enrollments.Where(e => e.Status != EnrollmentStatus.Cancelled).Average(e => e.ProgressPercent)
                };
            })
            .Where(c => c.TotalEnrolled >= 5)
            .OrderByDescending(c => c.CompletionRatePercent)
            .ToList();
    }

    public async Task<InstructorStatsDto> GetInstructorStatsAsync(string instructorId, string instructorName)
    {
        var courses = await context.Courses
            .AsNoTracking()
            .Include(c => c.Enrollments)
            .Include(c => c.Reviews)
            .Where(c => c.InstructorId == instructorId)
            .ToListAsync();

        var topCourses = courses
            .Select(c =>
            {
                var enrollments = c.Enrollments.Where(e => e.Status != EnrollmentStatus.Cancelled).ToList();
                var completed = c.Enrollments.Count(e => e.Status == EnrollmentStatus.Completed);

                return new TopCourseDto
                {
                    CourseId = c.Id,
                    Title = c.Title,
                    InstructorName = instructorName,
                    EnrollmentCount = enrollments.Count,
                    CompletedCount = completed,
                    CompletionRate = enrollments.Count == 0 ? 0 : completed * 100.0 / enrollments.Count,
                    AverageRating = c.Reviews.Count == 0 ? 0 : c.Reviews.Average(r => r.Rating),
                    Revenue = enrollments.Count * c.Price
                };
            })
            .OrderByDescending(c => c.EnrollmentCount)
            .Take(5)
            .ToList();

        var enrollmentTrend = courses
            .SelectMany(c => c.Enrollments.Select(e => new { Enrollment = e, CoursePrice = c.Price }))
            .GroupBy(x => new { x.Enrollment.EnrolledAt.Year, x.Enrollment.EnrolledAt.Month })
            .OrderBy(g => g.Key.Year)
            .ThenBy(g => g.Key.Month)
            .Select(g => new MonthlyEnrollmentDto
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                MonthName = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMMM"),
                NewEnrollments = g.Count(),
                Completions = g.Count(x => x.Enrollment.Status == EnrollmentStatus.Completed),
                Revenue = g.Where(x => x.Enrollment.Status != EnrollmentStatus.Cancelled).Sum(x => x.CoursePrice)
            })
            .ToList();

        var reviews = courses.SelectMany(c => c.Reviews).ToList();
        var totalStudents = courses
            .SelectMany(c => c.Enrollments)
            .Where(e => e.Status != EnrollmentStatus.Cancelled)
            .Select(e => e.StudentId)
            .Distinct()
            .Count();

        return new InstructorStatsDto
        {
            InstructorName = instructorName,
            CourseCount = courses.Count,
            PublishedCourseCount = courses.Count(c => c.IsPublished),
            TotalStudents = totalStudents,
            TotalReviews = reviews.Count,
            AverageRating = reviews.Count == 0 ? 0 : reviews.Average(r => r.Rating),
            TotalRevenue = courses.Sum(c => c.Enrollments.Count(e => e.Status != EnrollmentStatus.Cancelled) * c.Price),
            TopCourses = topCourses,
            EnrollmentTrend = enrollmentTrend
        };
    }

    public async Task<StudentsProgressSummaryDto> GetStudentsProgressAsync(int totalStudents)
    {
        var enrollments = await context.Enrollments
            .AsNoTracking()
            .Include(e => e.Student)
            .ToListAsync();

        var activeStudentIds = enrollments
            .Where(e => e.Status == EnrollmentStatus.Active)
            .Select(e => e.StudentId)
            .Distinct()
            .ToHashSet();

        var completedStudentIds = enrollments
            .Where(e => e.Status == EnrollmentStatus.Completed)
            .Select(e => e.StudentId)
            .Distinct()
            .ToHashSet();

        var topActiveStudents = enrollments
            .GroupBy(e => new { e.StudentId, e.Student.FullName })
            .Select(g => new StudentProgressDto
            {
                StudentId = g.Key.StudentId,
                FullName = g.Key.FullName,
                CompletedCourses = g.Count(e => e.Status == EnrollmentStatus.Completed),
                ActiveEnrollments = g.Count(e => e.Status == EnrollmentStatus.Active),
                AverageProgress = g.Average(e => e.ProgressPercent)
            })
            .OrderByDescending(s => s.ActiveEnrollments)
            .ThenByDescending(s => s.AverageProgress)
            .Take(10)
            .ToList();

        return new StudentsProgressSummaryDto
        {
            TotalStudents = totalStudents,
            StudentsWithActiveEnrollment = activeStudentIds.Count,
            StudentsCompletedAtLeastOne = completedStudentIds.Count,
            StudentsNeverStarted = totalStudents - enrollments.Select(e => e.StudentId).Distinct().Count(),
            AverageCoursesPerStudent = totalStudents == 0 ? 0 : enrollments.Count * 1.0 / totalStudents,
            TopActiveStudents = topActiveStudents
        };
    }

    public async Task<RatingsDistributionDto> GetRatingsDistributionAsync()
    {
        var reviews = await context.Reviews.AsNoTracking().ToListAsync();

        return new RatingsDistributionDto
        {
            OneStar = reviews.Count(r => r.Rating == 1),
            TwoStars = reviews.Count(r => r.Rating == 2),
            ThreeStars = reviews.Count(r => r.Rating == 3),
            FourStars = reviews.Count(r => r.Rating == 4),
            FiveStars = reviews.Count(r => r.Rating == 5),
            AverageRating = reviews.Count == 0 ? 0 : reviews.Average(r => r.Rating),
            TotalReviews = reviews.Count
        };
    }
}
