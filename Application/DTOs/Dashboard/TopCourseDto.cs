namespace Application.DTOs.Dashboard;

public class TopCourseDto
{
    public Guid CourseId { get; set; }
    public string Title { get; set; } = "";
    public string InstructorName { get; set; } = "";
    public int EnrollmentCount { get; set; }
    public int CompletedCount { get; set; }
    public double CompletionRate { get; set; }
    public double AverageRating { get; set; }
    public decimal Revenue { get; set; }
}
