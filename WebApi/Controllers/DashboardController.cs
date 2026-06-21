using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.Common;
using Application.DTOs.Dashboard;
using Application.Interfaces.Services;
using Domain.Constants;

namespace WebApi.Controllers;

[Route("api/dashboard")]
public class DashboardController(IDashboardService dashboardService) : BaseController
{
    [Authorize(Roles = UserRoles.Admin)]
    [HttpGet("summary")]
    public async Task<ActionResult<Result<DashboardSummaryDto>>> Summary()
    {
        var result = await dashboardService.GetSummaryAsync();
        return Ok(result);
    }

    [Authorize(Roles = UserRoles.Admin)]
    [HttpGet("top-courses")]
    public async Task<ActionResult<Result<IReadOnlyList<TopCourseDto>>>> TopCourses()
    {
        var result = await dashboardService.GetTopCoursesAsync();
        return Ok(result);
    }

    [Authorize(Roles = UserRoles.Admin)]
    [HttpGet("enrollments-by-month")]
    public async Task<ActionResult<Result<IReadOnlyList<MonthlyEnrollmentDto>>>> EnrollmentsByMonth()
    {
        var result = await dashboardService.GetEnrollmentsByMonthAsync();
        return Ok(result);
    }

    [Authorize(Roles = UserRoles.Admin)]
    [HttpGet("revenue-by-category")]
    public async Task<ActionResult<Result<IReadOnlyList<CategoryRevenueDto>>>> RevenueByCategory()
    {
        var result = await dashboardService.GetRevenueByCategoryAsync();
        return Ok(result);
    }

    [Authorize(Roles = UserRoles.Admin)]
    [HttpGet("completion-rate")]
    public async Task<ActionResult<Result<IReadOnlyList<CompletionRateDto>>>> CompletionRate()
    {
        var result = await dashboardService.GetCompletionRateAsync();
        return Ok(result);
    }

    [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Instructor}")]
    [HttpGet("instructor/{instructorId}")]
    public async Task<ActionResult<Result<InstructorStatsDto>>> InstructorStats(string instructorId)
    {
        var result = await dashboardService.GetInstructorStatsAsync(instructorId, UserId, IsAdmin);
        return Ok(result);
    }

    [Authorize(Roles = UserRoles.Admin)]
    [HttpGet("students-progress")]
    public async Task<ActionResult<Result<StudentsProgressSummaryDto>>> StudentsProgress()
    {
        var result = await dashboardService.GetStudentsProgressAsync();
        return Ok(result);
    }

    [Authorize(Roles = UserRoles.Admin)]
    [HttpGet("ratings-distribution")]
    public async Task<ActionResult<Result<RatingsDistributionDto>>> RatingsDistribution()
    {
        var result = await dashboardService.GetRatingsDistributionAsync();
        return Ok(result);
    }
}
