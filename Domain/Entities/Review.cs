namespace Domain.Entities;

public class Review
{
    public Guid Id { get; set; }
    public string StudentId { get; set; } = string.Empty;
    public Guid CourseId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public ApplicationUser Student { get; set; } = null!;
    public Course Course { get; set; } = null!;
}
