using Application.DTOs.Courses;
using Application.Interfaces.Services;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MyRazor.Pages.Courses;

public class EditModel(ICourseService courseService, AppDbContext context) : PageModel
{
    [BindProperty]
    public UpdateCourseDto Dto { get; set; } = new();
    public List<SelectListItem> Categories { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        await LoadCategoriesAsync();

        var course = await context.Courses
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (course is null)
            return NotFound();

        Dto = new UpdateCourseDto
        {
            Title = course.Title,
            Description = course.Description,
            Price = course.Price,
            Level = course.Level,
            CategoryId = course.CategoryId
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid id)
    {
        await LoadCategoriesAsync();

        if (!ModelState.IsValid)
            return Page();

        var result = await courseService.UpdateAsync(id, Dto);

        if (result.IsSuccess)
            return RedirectToPage("./Index");

        ModelState.AddModelError(string.Empty, result.Message ?? "Course was not updated.");

        return Page();
    }


    private async Task LoadCategoriesAsync()
{
    Categories = await context.Categories
        .OrderBy(x => x.Name)
        .Select(x => new SelectListItem
        {
            Value = x.Id.ToString(),
            Text = x.Name
        })
        .ToListAsync();
}
}
