using Application.DTOs.Courses;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyRazor.Models;

namespace MyRazor.Pages.Courses;

public class IndexModel : PageModel
{
    private readonly ICourseService _courseService;

    public List<CourseDto> Courses { get; set; } = new();
    public StudentProfileViewModel Student { get; set; } = new();
    public List<StudentCourseProgressViewModel> StudentCourses { get; set; } = new();
    public Course Course { get; set; } = new();
    public List<string> WhatYouLearn { get; set; } = new();
    public bool IsEnrolled { get; set; } = true;

    public string LevelDisplayName => Course.Level.ToString();
    public int EnrollmentCount => Course.Enrollments.Count;
    public int ReviewCount => Course.Reviews.Count;
    public double AverageRating => ReviewCount == 0 ? 0 : Course.Reviews.Average(r => r.Rating);

    public IndexModel(ICourseService courseService)
    {
        _courseService = courseService;
    }

    public async Task OnGetAsync()
    {
        LoadMockStudentData();
        LoadMockCourseDetail();

        var result = await _courseService.GetAllCourseAsync();
        Courses = result.Value ?? [];

        if (Courses.Count == 0)
        {
            Courses = StudentCourses.Select(course => new CourseDto
            {
                Id = course.CourseId,
                Title = course.Title,
                Description = course.Description,
                Price = course.Price,
                Level = course.Level,
                IsPublished = true,
                CategoryName = course.Category,
                InstructorName = course.InstructorName,
                StudentsCount = course.TotalStudents,
                LessonsCount = 12,
                ReviewsCount = 8,
                EnrollmentsCount = course.TotalStudents,
                CreatedAt = course.EnrolledAt.AddDays(-21)
            }).ToList();
        }
    }

    public int GetStarPercent(int star)
    {
        if (ReviewCount == 0)
            return 0;

        var count = Course.Reviews.Count(review => review.Rating == star);
        return (int)Math.Round(count * 100d / ReviewCount);
    }

    private void LoadMockStudentData()
    {
        Student = new StudentProfileViewModel
        {
            Id = "ST-2048",
            FullName = "Aziz Karimov",
            Email = "aziz.karimov@student.com",
            Phone = "+992 93 777 45 21",
            City = "Dushanbe",
            AvatarUrl = "https://i.pravatar.cc/160?img=12",
            CurrentGoal = "Become a backend ASP.NET developer",
            LearningStreakDays = 18,
            CompletedLessons = 42,
            Certificates = 3,
            AverageProgress = 68
        };

        StudentCourses = new List<StudentCourseProgressViewModel>
        {
            new()
            {
                CourseId = Guid.Parse("10000000-0000-0000-0000-000000000001"),
                Title = "ASP.NET Core Web API Masterclass",
                Description = "Clean Architecture, Identity, JWT and PostgreSQL.",
                Category = "Programming",
                InstructorName = "Farid Safarov",
                Level = CourseLevel.Intermediate,
                Price = 79,
                ProgressPercent = 84,
                Status = "Active",
                NextLesson = "JWT refresh tokens and claims",
                EnrolledAt = DateTime.UtcNow.AddDays(-32),
                TotalStudents = 128
            },
            new()
            {
                CourseId = Guid.Parse("10000000-0000-0000-0000-000000000002"),
                Title = "Entity Framework Core with PostgreSQL",
                Description = "Migrations, Fluent API, relationships and queries.",
                Category = "Database",
                InstructorName = "Madina Yusufzoda",
                Level = CourseLevel.Beginner,
                Price = 49,
                ProgressPercent = 61,
                Status = "Active",
                NextLesson = "Many-to-many relationships",
                EnrolledAt = DateTime.UtcNow.AddDays(-18),
                TotalStudents = 94
            },
            new()
            {
                CourseId = Guid.Parse("10000000-0000-0000-0000-000000000003"),
                Title = "Redis Caching for .NET Apps",
                Description = "Dashboard cache, invalidation and performance patterns.",
                Category = "Performance",
                InstructorName = "Said Nazarov",
                Level = CourseLevel.Advanced,
                Price = 59,
                ProgressPercent = 38,
                Status = "In Progress",
                NextLesson = "Cache invalidation strategy",
                EnrolledAt = DateTime.UtcNow.AddDays(-7),
                TotalStudents = 57
            }
        };
    }

    private void LoadMockCourseDetail()
    {
        var courseId = Guid.Parse("10000000-0000-0000-0000-000000000001");
        var categoryId = Guid.Parse("20000000-0000-0000-0000-000000000001");

        var instructor = new ApplicationUser
        {
            Id = "instructor-1",
            FullName = "Farid Safarov",
            UserName = "Farid Safarov",
            Email = "farid.safarov@onlinecourses.local"
        };

        var category = new Category
        {
            Id = categoryId,
            Name = "Programming",
            Description = "Backend and software development"
        };

        Course = new Course
        {
            Id = courseId,
            Title = "ASP.NET Core Web API Masterclass",
            Description = "Build real Web APIs with Clean Architecture, Identity, JWT authentication, PostgreSQL, Redis caching and a clean service layer.",
            Price = 79,
            Level = CourseLevel.Intermediate,
            IsPublished = true,
            ThumbnailUrl = "https://images.unsplash.com/photo-1516321318423-f06f85e504b3?auto=format&fit=crop&w=1200&q=80",
            InstructorId = instructor.Id,
            Instructor = instructor,
            CategoryId = category.Id,
            Category = category,
            CreatedAt = DateTime.UtcNow.AddMonths(-2),
            PublishedAt = DateTime.UtcNow.AddMonths(-1),
            UpdatedAt = DateTime.UtcNow.AddDays(-4)
        };

        Course.Lessons = new List<Lesson>
        {
            new() { Id = Guid.NewGuid(), CourseId = courseId, Course = Course, Order = 1, Title = "Project structure and Clean Architecture", DurationMinutes = 18, CreatedAt = DateTime.UtcNow.AddDays(-45) },
            new() { Id = Guid.NewGuid(), CourseId = courseId, Course = Course, Order = 2, Title = "Domain entities and relationships", DurationMinutes = 24, CreatedAt = DateTime.UtcNow.AddDays(-43) },
            new() { Id = Guid.NewGuid(), CourseId = courseId, Course = Course, Order = 3, Title = "Identity registration and login", DurationMinutes = 31, CreatedAt = DateTime.UtcNow.AddDays(-39) },
            new() { Id = Guid.NewGuid(), CourseId = courseId, Course = Course, Order = 4, Title = "JWT bearer authentication", DurationMinutes = 27, CreatedAt = DateTime.UtcNow.AddDays(-35) },
            new() { Id = Guid.NewGuid(), CourseId = courseId, Course = Course, Order = 5, Title = "Dashboard cache with Redis", DurationMinutes = 22, CreatedAt = DateTime.UtcNow.AddDays(-28) }
        };

        Course.Enrollments = new List<Enrollment>
        {
            new() { Id = Guid.NewGuid(), CourseId = courseId, Course = Course, StudentId = "student-1", ProgressPercent = 84, EnrolledAt = DateTime.UtcNow.AddDays(-32) },
            new() { Id = Guid.NewGuid(), CourseId = courseId, Course = Course, StudentId = "student-2", ProgressPercent = 72, EnrolledAt = DateTime.UtcNow.AddDays(-25) },
            new() { Id = Guid.NewGuid(), CourseId = courseId, Course = Course, StudentId = "student-3", ProgressPercent = 43, EnrolledAt = DateTime.UtcNow.AddDays(-14) },
            new() { Id = Guid.NewGuid(), CourseId = courseId, Course = Course, StudentId = "student-4", ProgressPercent = 91, EnrolledAt = DateTime.UtcNow.AddDays(-9) }
        };

        Course.Reviews = new List<Review>
        {
            new()
            {
                Id = Guid.NewGuid(),
                CourseId = courseId,
                Course = Course,
                StudentId = "student-1",
                Student = new ApplicationUser { Id = "student-1", FullName = "Aziz Karimov", UserName = "Aziz Karimov" },
                Rating = 5,
                Comment = "Clear lessons, practical examples and a very useful project structure.",
                CreatedAt = DateTime.UtcNow.AddDays(-8)
            },
            new()
            {
                Id = Guid.NewGuid(),
                CourseId = courseId,
                Course = Course,
                StudentId = "student-2",
                Student = new ApplicationUser { Id = "student-2", FullName = "Madina Saidova", UserName = "Madina Saidova" },
                Rating = 4,
                Comment = "Good course for understanding Identity and JWT in a real project.",
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            },
            new()
            {
                Id = Guid.NewGuid(),
                CourseId = courseId,
                Course = Course,
                StudentId = "student-3",
                Student = new ApplicationUser { Id = "student-3", FullName = "Rustam Nazarov", UserName = "Rustam Nazarov" },
                Rating = 5,
                Comment = "The Redis dashboard part helped me a lot.",
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            }
        };

        WhatYouLearn = new List<string>
        {
            "Create a clean ASP.NET Core Web API structure",
            "Use Identity for users, roles and passwords",
            "Generate and validate JWT bearer tokens",
            "Work with EF Core and PostgreSQL",
            "Build course, lesson, enrollment and review logic",
            "Cache dashboard data with Redis"
        };
    }
}
