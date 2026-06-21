using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.Common;
using Application.DTOs.Courses;
using Application.Interfaces.Services;
using Domain.Constants;

namespace WebApi.Controllers;

[Route("api/courses")]
public class CoursesController(ICourseService courseService) : BaseController
{
    [HttpGet]
    public async Task<ActionResult<Result<List<CourseDto>>>> Get([FromQuery] CourseQueryDto query)
    {
        var result = await courseService.GetAsync(query);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Result<CourseDto>>> GetById(Guid id)
    {
        var result = await courseService.GetByIdAsync(id);
        return Ok(result);
    }

    [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Instructor}")]
    [HttpPost]
    public async Task<ActionResult<Result<CourseDto>>> Create(CreateCourseDto dto)
    {
        var result = await courseService.CreateAsync(UserId, dto);
        return Ok(result);
    }

    [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Instructor}")]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<Result<CourseDto>>> Update(Guid id, UpdateCourseDto dto)
    {
        var result = await courseService.UpdateAsync(id, UserId, IsAdmin, dto);
        return Ok(result);
    }

    [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Instructor}")]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<Result>> Delete(Guid id)
    {
        var result = await courseService.DeleteAsync(id, UserId, IsAdmin);
        return Ok(result);
    }

    [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Instructor}")]
    [HttpPatch("{id:guid}/publish")]
    public async Task<ActionResult<Result<CourseDto>>> Publish(Guid id)
    {
        var result = await courseService.PublishAsync(id, UserId, IsAdmin);
        return Ok(result);
    }

    [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Instructor}")]
    [HttpPost("{id:guid}/thumbnail")]
    public async Task<ActionResult<Result<CourseDto>>> UploadThumbnail(Guid id, [FromForm] IFormFile file)
    {
        var result = await courseService.UploadThumbnailAsync(id, UserId, IsAdmin, file);
        return Ok(result);
    }
}
