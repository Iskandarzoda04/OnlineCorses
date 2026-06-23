using Application.Common;
using Application.DTOs.Dashboard;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Constants;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IDashboardRepository _dashboardRepository;
    private readonly ICacheService _cache;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<DashboardService> _logger;

    public DashboardService(
        IDashboardRepository dashboardRepository,
        ICacheService cache,
        UserManager<ApplicationUser> userManager,
        ILogger<DashboardService> logger)
    {
        _dashboardRepository = dashboardRepository;
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
            TotalCourses = await _dashboardRepository.GetTotalCoursesAsync(),
            PublishedCourses = await _dashboardRepository.GetPublishedCoursesAsync(),
            TotalStudents = students.Count,
            TotalInstructors = instructors.Count,
            TotalEnrollments = await _dashboardRepository.GetTotalEnrollmentsAsync(),
            ActiveEnrollments = await _dashboardRepository.GetActiveEnrollmentsAsync(),
            CompletedEnrollments = await _dashboardRepository.GetCompletedEnrollmentsAsync(),
            TotalRevenue = await _dashboardRepository.GetTotalRevenueAsync(),
            AveragePlatformRating = await _dashboardRepository.GetAverageRatingAsync(),
            TotalReviews = await _dashboardRepository.GetTotalReviewsAsync()
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

        var topCourses = await _dashboardRepository.GetTopCoursesAsync();

        await _cache.SetAsync(cacheKey, topCourses, TimeSpan.FromMinutes(30));
        _logger.LogInformation("Dashboard top courses created and cached.");

        return Result<IReadOnlyList<TopCourseDto>>.Success(topCourses);
    }

    public async Task<Result<IReadOnlyList<MonthlyEnrollmentDto>>> GetEnrollmentsByMonthAsync()
    {
        var result = await _dashboardRepository.GetEnrollmentsByMonthAsync();
        return Result<IReadOnlyList<MonthlyEnrollmentDto>>.Success(result);
    }

    public async Task<Result<IReadOnlyList<CategoryRevenueDto>>> GetRevenueByCategoryAsync()
    {
        var result = await _dashboardRepository.GetRevenueByCategoryAsync();
        return Result<IReadOnlyList<CategoryRevenueDto>>.Success(result);
    }

    public async Task<Result<IReadOnlyList<CompletionRateDto>>> GetCompletionRateAsync()
    {
        var result = await _dashboardRepository.GetCompletionRateAsync();
        return Result<IReadOnlyList<CompletionRateDto>>.Success(result);
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

        var result = await _dashboardRepository.GetInstructorStatsAsync(instructorId, instructor.FullName);
        return Result<InstructorStatsDto>.Success(result);
    }

    public async Task<Result<StudentsProgressSummaryDto>> GetStudentsProgressAsync()
    {
        var students = await _userManager.GetUsersInRoleAsync(UserRoles.Student);
        var result = await _dashboardRepository.GetStudentsProgressAsync(students.Count);
        return Result<StudentsProgressSummaryDto>.Success(result);
    }

    public async Task<Result<RatingsDistributionDto>> GetRatingsDistributionAsync()
    {
        var result = await _dashboardRepository.GetRatingsDistributionAsync();
        return Result<RatingsDistributionDto>.Success(result);
    }
}
