using Application.DTOs.Courses;
using Application.Interfaces.Services;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MyRazor.Pages.Courses;

public class CreateModel(
    ICourseService courseService,
    AppDbContext context,
    UserManager<ApplicationUser> userManager) : PageModel
{
    [BindProperty]
    public CreateCourseDto Dto { get; set; } = new();

    public List<SelectListItem> Categories { get; set; } = new();

    public async Task OnGetAsync()
    {
        await LoadCategoriesAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadCategoriesAsync();

        if (!ModelState.IsValid)
            return Page();

        var instructorId = await GetDefaultInstructorIdAsync();
        var result = await courseService.CreateAsync(instructorId, Dto);

        if (result.IsSuccess)
            return RedirectToPage("./Index");

        ModelState.AddModelError(string.Empty, result.Message ?? "Course was not created.");

        return Page();
    }

    private async Task LoadCategoriesAsync()
    {
        await EnsureDefaultCategoriesAsync();

        Categories = await context.Categories
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.Name
            })
            .ToListAsync();
    }

    private async Task EnsureDefaultCategoriesAsync()
    {
        if (await context.Categories.AnyAsync())
            return;

        context.Categories.AddRange(
            new Category { Name = "Programming", Description = "Software development courses" },
            new Category { Name = "Database", Description = "Database and persistence courses" },
            new Category { Name = "Frontend", Description = "Frontend development courses" },
            new Category { Name = "Backend", Description = "Backend development courses" });

        await context.SaveChangesAsync();
    }

    private async Task<string> GetDefaultInstructorIdAsync()
    {
        const string email = "instructor@example.com";

        var instructor = await userManager.FindByEmailAsync(email);
        if (instructor is not null)
            return instructor.Id;

        instructor = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = "Default Instructor",
            EmailConfirmed = true
        };

        await userManager.CreateAsync(instructor, "Instructor123!");
        return instructor.Id;
    }
}
