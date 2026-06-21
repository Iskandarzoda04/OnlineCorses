using Domain.Enums;

namespace Application.DTOs.Courses;

public class CourseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public CourseLevel Level { get; set; }
    public bool IsPublished { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string InstructorId { get; set; } = "";
    public string InstructorName { get; set; } = "";
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = "";
    public double AverageRating { get; set; }
    public int EnrollmentsCount { get; set; }
}
