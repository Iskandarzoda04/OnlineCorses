namespace MyRazor.Models;

public class StudentProfileViewModel
{
    public string Id { get; set; } = "";
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public string City { get; set; } = "";
    public string AvatarUrl { get; set; } = "";
    public string CurrentGoal { get; set; } = "";
    public int LearningStreakDays { get; set; }
    public int CompletedLessons { get; set; }
    public int Certificates { get; set; }
    public int AverageProgress { get; set; }
}
