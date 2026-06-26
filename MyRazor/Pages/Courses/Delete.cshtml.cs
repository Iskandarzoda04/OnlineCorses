using Application.DTOs.Courses;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MyRazor.Pages.Courses;

public class Delete(ICourseService courseService) : PageModel
{
    [BindProperty]
    public CourseDto Course { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var result = await courseService.GetByIdAsync(id);

        if (!result.IsSuccess || result.Value is null)
            return NotFound();

        Course = result.Value;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid id)
    {
        var deleteResult = await courseService.DeleteAsync(id, string.Empty, true);

        if (deleteResult.IsSuccess)
            return RedirectToPage("./Index");

        var courseResult = await courseService.GetByIdAsync(id);

        if (courseResult.IsSuccess && courseResult.Value is not null)
            Course = courseResult.Value;
        else
            return NotFound();

        ModelState.AddModelError(string.Empty, deleteResult.Message ?? "Course was not deleted.");
        return Page();
    }
}