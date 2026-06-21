using Domain.Enums;

namespace Domain.Entities;

public class Enrollment
{
    public Guid Id { get; set; }
    public string StudentId { get; set; } = string.Empty;
    public Guid CourseId { get; set; }
    public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Active;
    public int ProgressPercent { get; set; }
    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }

    public ApplicationUser Student { get; set; } = null!;
    public Course Course { get; set; } = null!;
}
