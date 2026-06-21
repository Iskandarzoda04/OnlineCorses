using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.Common;
using Application.DTOs.Lessons;
using Application.Interfaces.Services;
using Domain.Constants;

namespace WebApi.Controllers;

[Route("api/courses/{courseId:guid}/lessons")]
public class LessonsController(ILessonService lessonService) : BaseController
{
    [HttpGet]
    public async Task<ActionResult<Result<IReadOnlyList<LessonDto>>>> GetByCourse(Guid courseId)
    {
        var result = await lessonService.GetByCourseAsync(courseId);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Result<LessonDto>>> GetById(Guid courseId, Guid id)
    {
        var result = await lessonService.GetByIdAsync(courseId, id);
        return Ok(result);
    }

    [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Instructor}")]
    [HttpPost]
    public async Task<ActionResult<Result<LessonDto>>> Create(Guid courseId, CreateLessonDto dto)
    {
        dto.CourseId = courseId;
        var result = await lessonService.CreateAsync(UserId, IsAdmin, dto);
        return Ok(result);
    }

    [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Instructor}")]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<Result<LessonDto>>> Update(Guid id, UpdateLessonDto dto)
    {
        var result = await lessonService.UpdateAsync(id, UserId, IsAdmin, dto);
        return Ok(result);
    }

    [Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Instructor}")]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<Result>> Delete(Guid id)
    {
        var result = await lessonService.DeleteAsync(id, UserId, IsAdmin);
        return Ok(result);
    }
}
