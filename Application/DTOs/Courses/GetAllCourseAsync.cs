namespace Application.DTOs.Courses;


public class GetAllCourseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = "";
    public decimal Price { get; set; }
    public string CategoryName { get; set; } = "";
}