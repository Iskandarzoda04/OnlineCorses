using Application.Common;
using Application.DTOs.Dashboard;

namespace Application.Interfaces.Services;

public interface IDashboardService
{
    Task<Result<DashboardSummaryDto>> GetSummaryAsync();
    Task<Result<IReadOnlyList<TopCourseDto>>> GetTopCoursesAsync();
    Task<Result<IReadOnlyList<MonthlyEnrollmentDto>>> GetEnrollmentsByMonthAsync();
    Task<Result<IReadOnlyList<CategoryRevenueDto>>> GetRevenueByCategoryAsync();
    Task<Result<IReadOnlyList<CompletionRateDto>>> GetCompletionRateAsync();
    Task<Result<InstructorStatsDto>> GetInstructorStatsAsync(string instructorId, string currentUserId, bool isAdmin);
    Task<Result<StudentsProgressSummaryDto>> GetStudentsProgressAsync();
    Task<Result<RatingsDistributionDto>> GetRatingsDistributionAsync();
}
