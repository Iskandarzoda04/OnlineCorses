using Domain.Enums;

namespace Application.DTOs.Enrollments;

public class EnrollmentDto
{
    public Guid Id { get; set; }
    public string StudentId { get; set; } = "";
    public Guid CourseId { get; set; }
    public string CourseTitle { get; set; } = "";
    public EnrollmentStatus Status { get; set; }
    public int ProgressPercent { get; set; }
    public DateTime EnrolledAt { get; set; }
}
