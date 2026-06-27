using Application.DTOs.Courses;
using Application.Interfaces.Services;
using Domain.Constants;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MvcAAp.Models;

namespace MvcAAp.Controllers;

public class CourseController(
    ICourseService courseService,
    AppDbContext context,
    UserManager<ApplicationUser> userManager,
    IConfiguration configuration) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        return View(await BuildViewModelAsync());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(CoursePagedViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            return View(await BuildViewModelAsync(
                viewModel.Dto,
                true,
                "Please fix the errors in the form"));
        }

        var instructorId = await GetInstructorIdAsync();
        if (string.IsNullOrWhiteSpace(instructorId))
        {
            return View(await BuildViewModelAsync(
                viewModel.Dto,
                true,
                "Instructor user not found. Create an admin or instructor first."));
        }

        var result = await courseService.CreateAsync(instructorId, viewModel.Dto);

        if (!result.IsSuccess)
        {
            return View(await BuildViewModelAsync(
                viewModel.Dto,
                true,
                result.Message ?? "Course was not created"));
        }

        TempData["SuccessMessage"] = $"Course \"{result.Value?.Title}\" created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Publish(Guid id)
    {
        var userId = await GetInstructorIdAsync();
        if (string.IsNullOrWhiteSpace(userId))
        {
            TempData["ErrorMessage"] = "User not found.";
            return RedirectToAction(nameof(Index));
        }

        var result = await courseService.PublishAsync(id, userId, IsAdminRequest());
        TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = result.Message
            ?? (result.IsSuccess ? "Course status changed." : "Course status was not changed.");

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, UpdateCourseDto dto)
    {
        var result = await courseService.UpdateAsync(id, dto);
        TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = result.Message
            ?? (result.IsSuccess ? "Course updated successfully." : "Course was not updated.");

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = await GetInstructorIdAsync();
        if (string.IsNullOrWhiteSpace(userId))
        {
            TempData["ErrorMessage"] = "User not found.";
            return RedirectToAction(nameof(Index));
        }

        var result = await courseService.DeleteAsync(id, userId, IsAdminRequest());
        TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = result.Message
            ?? (result.IsSuccess ? "Course deleted." : "Course was not deleted.");

        return RedirectToAction(nameof(Index));
    }

    private async Task<CoursePagedViewModel> BuildViewModelAsync(
        CreateCourseDto? dto = null,
        bool showModal = false,
        string? errorMessage = null)
    {
        var result = await courseService.GetAllCourseAsync();
        var categories = await context.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            })
            .ToListAsync();

        return new CoursePagedViewModel
        {
            Courses = result.Value ?? [],
            Dto = dto ?? new CreateCourseDto(),
            Categories = categories,
            ShowModal = showModal,
            ErrorMessage = errorMessage
                ?? TempData["ErrorMessage"] as string
                ?? (!result.IsSuccess ? result.Message : null)
        };
    }

    private bool IsAdminRequest()
    {
        return User.Identity?.IsAuthenticated != true || User.IsInRole(UserRoles.Admin);
    }

    private async Task<string?> GetInstructorIdAsync()
    {
        var userId = userManager.GetUserId(User);
        if (!string.IsNullOrWhiteSpace(userId))
            return userId;

        var adminEmail = configuration["DefaultAdmin:Email"] ?? "admin@example.com";
        var admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin is not null)
            return admin.Id;

        return await context.Users
            .AsNoTracking()
            .OrderBy(u => u.CreatedAt)
            .Select(u => u.Id)
            .FirstOrDefaultAsync();
    }
}
