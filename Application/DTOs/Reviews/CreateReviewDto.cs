namespace Application.DTOs.Reviews;

public class CreateReviewDto
{
    public Guid CourseId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
}
