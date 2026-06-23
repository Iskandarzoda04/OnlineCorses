using Application.DTOs.Dashboard;

namespace Application.Interfaces.Repositories;

public interface IDashboardRepository
{
    Task<int> GetTotalCoursesAsync();
    Task<int> GetPublishedCoursesAsync();
    Task<int> GetTotalEnrollmentsAsync();
    Task<int> GetActiveEnrollmentsAsync();
    Task<int> GetCompletedEnrollmentsAsync();
    Task<decimal> GetTotalRevenueAsync();
    Task<double> GetAverageRatingAsync();
    Task<int> GetTotalReviewsAsync();
    Task<IReadOnlyList<TopCourseDto>> GetTopCoursesAsync();
    Task<IReadOnlyList<MonthlyEnrollmentDto>> GetEnrollmentsByMonthAsync();
    Task<IReadOnlyList<CategoryRevenueDto>> GetRevenueByCategoryAsync();
    Task<IReadOnlyList<CompletionRateDto>> GetCompletionRateAsync();
    Task<InstructorStatsDto> GetInstructorStatsAsync(string instructorId, string instructorName);
    Task<StudentsProgressSummaryDto> GetStudentsProgressAsync(int totalStudents);
    Task<RatingsDistributionDto> GetRatingsDistributionAsync();
}
