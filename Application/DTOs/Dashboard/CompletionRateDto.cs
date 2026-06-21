namespace Application.DTOs.Dashboard;

public class CompletionRateDto
{
    public Guid CourseId { get; set; }
    public string Title { get; set; } = "";
    public int TotalEnrolled { get; set; }
    public int TotalCompleted { get; set; }
    public double CompletionRatePercent { get; set; }
    public double AverageProgressPercent { get; set; }
}
