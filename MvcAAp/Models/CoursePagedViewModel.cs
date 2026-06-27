using Application.DTOs.Courses;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MvcAAp.Models;

public class CoursePagedViewModel
{
    [ValidateNever]
    public List<CourseDto> Courses { get; set; } = [];

    public CreateCourseDto Dto { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public bool ShowModal { get; set; }

    [ValidateNever]
    public List<SelectListItem> Categories { get; set; } = [];
}
