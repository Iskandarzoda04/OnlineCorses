namespace Application.DTOs.Dashboard;

public class StudentProgressDto
{
    public string StudentId { get; set; } = "";
    public string FullName { get; set; } = "";
    public int CompletedCourses { get; set; }
    public int ActiveEnrollments { get; set; }
    public double AverageProgress { get; set; }
}
