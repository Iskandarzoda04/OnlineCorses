using Domain.Enums;

namespace MyRazor.Models;

public class StudentCourseProgressViewModel
{
    public Guid CourseId { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string Category { get; set; } = "";
    public string InstructorName { get; set; } = "";
    public CourseLevel Level { get; set; }
    public decimal Price { get; set; }
    public int ProgressPercent { get; set; }
    public string Status { get; set; } = "";
    public string NextLesson { get; set; } = "";
    public DateTime EnrolledAt { get; set; }
    public int TotalStudents { get; set; }
}
