using Domain.Enums;

namespace Application.DTOs.Courses;

public class UpdateCourseDto
{
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public CourseLevel Level { get; set; }
    public Guid CategoryId { get; set; }
}
