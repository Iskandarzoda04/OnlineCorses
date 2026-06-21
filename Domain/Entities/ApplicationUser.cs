using Microsoft.AspNetCore.Identity;

namespace Domain.Entities;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<Course> Courses { get; set; } = new();
    public List<Enrollment> Enrollments { get; set; } = new();
    public List<Review> Reviews { get; set; } = new();
}
