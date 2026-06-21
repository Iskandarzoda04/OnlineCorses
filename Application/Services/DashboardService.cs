using Application.Common;
using Application.DTOs.Dashboard;
using Application.Interfaces.Services;
using Domain.Constants;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class DashboardService : IDashboardService
{
    private readonly ICacheService _cache;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<DashboardService> _logger;

    public DashboardService(
        ICacheService cache,
        UserManager<ApplicationUser> userManager,
        ILogger<DashboardService> logger)
    {
        _cache = cache;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<Result<DashboardSummaryDto>> GetSummaryAsync()
    {
        var cacheKey = "dashboard:summary";
        var cached = await _cache.GetAsync<DashboardSummaryDto>(cacheKey);

        if (cached is not null)
            return Result<DashboardSummaryDto>.Success(cached);

        var students = await _userManager.GetUsersInRoleAsync(UserRoles.Student);
        var instructors = await _userManager.GetUsersInRoleAsync(UserRoles.Instructor);

        var summary = new DashboardSummaryDto
        {
            TotalCourses = 0,
            PublishedCourses = 0,
            TotalStudents = students.Count,
            TotalInstructors = instructors.Count,
            TotalEnrollments = 0,
            ActiveEnrollments = 0,
            CompletedEnrollments = 0,
            TotalRevenue = 0,
            AveragePlatformRating = 0,
            TotalReviews = 0
        };

        await _cache.SetAsync(cacheKey, summary, TimeSpan.FromMinutes(5));
        _logger.LogInformation("Dashboard summary created and cached.");

        return Result<DashboardSummaryDto>.Success(summary);
    }

    public async Task<Result<IReadOnlyList<TopCourseDto>>> GetTopCoursesAsync()
    {
        var cacheKey = "dashboard:top-courses";
        var cached = await _cache.GetAsync<IReadOnlyList<TopCourseDto>>(cacheKey);

        if (cached is not null)
            return Result<IReadOnlyList<TopCourseDto>>.Success(cached);

        IReadOnlyList<TopCourseDto> topCourses = new List<TopCourseDto>();

        await _cache.SetAsync(cacheKey, topCourses, TimeSpan.FromMinutes(30));
        _logger.LogInformation("Dashboard top courses created and cached.");

        return Result<IReadOnlyList<TopCourseDto>>.Success(topCourses);
    }

    public Task<Result<IReadOnlyList<MonthlyEnrollmentDto>>> GetEnrollmentsByMonthAsync()
    {
        IReadOnlyList<MonthlyEnrollmentDto> enrollments = new List<MonthlyEnrollmentDto>();
        return Task.FromResult(Result<IReadOnlyList<MonthlyEnrollmentDto>>.Success(enrollments));
    }

    public Task<Result<IReadOnlyList<CategoryRevenueDto>>> GetRevenueByCategoryAsync()
    {
        IReadOnlyList<CategoryRevenueDto> revenue = new List<CategoryRevenueDto>();
        return Task.FromResult(Result<IReadOnlyList<CategoryRevenueDto>>.Success(revenue));
    }

    public Task<Result<IReadOnlyList<CompletionRateDto>>> GetCompletionRateAsync()
    {
        IReadOnlyList<CompletionRateDto> completionRates = new List<CompletionRateDto>();
        return Task.FromResult(Result<IReadOnlyList<CompletionRateDto>>.Success(completionRates));
    }

    public async Task<Result<InstructorStatsDto>> GetInstructorStatsAsync(string instructorId, string currentUserId, bool isAdmin)
    {
        if (string.IsNullOrWhiteSpace(instructorId))
            return Result<InstructorStatsDto>.Failure("InstructorId is required.", ErrorType.Validation);

        if (!isAdmin && instructorId != currentUserId)
            return Result<InstructorStatsDto>.Failure("You can view only your own statistics.", ErrorType.Forbidden);

        var instructor = await _userManager.FindByIdAsync(instructorId);

        if (instructor is null)
            return Result<InstructorStatsDto>.Failure("Instructor not found.", ErrorType.NotFound);

        var isInstructor = await _userManager.IsInRoleAsync(instructor, UserRoles.Instructor);

        if (!isInstructor)
            return Result<InstructorStatsDto>.Failure("User is not instructor.", ErrorType.Validation);

        var stats = new InstructorStatsDto
        {
            InstructorName = instructor.FullName,
            CourseCount = 0,
            PublishedCourseCount = 0,
            TotalStudents = 0,
            TotalReviews = 0,
            AverageRating = 0,
            TotalRevenue = 0,
            TopCourses = new List<TopCourseDto>(),
            EnrollmentTrend = new List<MonthlyEnrollmentDto>()
        };

        return Result<InstructorStatsDto>.Success(stats);
    }

    public async Task<Result<StudentsProgressSummaryDto>> GetStudentsProgressAsync()
    {
        var totalStudents = await _userManager.GetUsersInRoleAsync(UserRoles.Student);

        var summary = new StudentsProgressSummaryDto
        {
            TotalStudents = totalStudents.Count,
            StudentsWithActiveEnrollment = 0,
            StudentsCompletedAtLeastOne = 0,
            StudentsNeverStarted = totalStudents.Count,
            AverageCoursesPerStudent = 0,
            TopActiveStudents = new List<StudentProgressDto>()
        };

        return Result<StudentsProgressSummaryDto>.Success(summary);
    }

    public Task<Result<RatingsDistributionDto>> GetRatingsDistributionAsync()
    {
        var ratings = new RatingsDistributionDto
        {
            OneStar = 0,
            TwoStars = 0,
            ThreeStars = 0,
            FourStars = 0,
            FiveStars = 0,
            AverageRating = 0,
            TotalReviews = 0
        };

        return Task.FromResult(Result<RatingsDistributionDto>.Success(ratings));
    }
}
