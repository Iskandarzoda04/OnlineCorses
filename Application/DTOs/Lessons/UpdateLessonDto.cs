namespace Application.DTOs.Lessons;

public class UpdateLessonDto
{
    public string Title { get; set; } = "";
    public string? Content { get; set; }
    public string? VideoUrl { get; set; }
    public int Order { get; set; }
    public int DurationMinutes { get; set; }
}
