using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.Common;
using Application.DTOs.Enrollments;
using Application.Interfaces.Services;
using Domain.Constants;

namespace WebApi.Controllers;

[Route("api/enrollments")]
public class EnrollmentsController(IEnrollmentService enrollmentService) : BaseController
{
    [Authorize(Roles = UserRoles.Student)]
    [HttpPost]
    public async Task<ActionResult<Result<EnrollmentDto>>> Enroll(CreateEnrollmentDto dto)
    {
        var result = await enrollmentService.EnrollAsync(UserId, dto.CourseId);
        return Ok(result);
    }

    [Authorize(Roles = UserRoles.Student)]
    [HttpGet("my")]
    public async Task<ActionResult<Result<IReadOnlyList<EnrollmentDto>>>> Mine()
    {
        var result = await enrollmentService.GetMineAsync(UserId);
        return Ok(result);
    }

    [Authorize(Roles = UserRoles.Admin)]
    [HttpGet]
    public async Task<ActionResult<Result<IReadOnlyList<EnrollmentDto>>>> GetAll()
    {
        var result = await enrollmentService.GetAllAsync();
        return Ok(result);
    }

    [Authorize(Roles = UserRoles.Student)]
    [HttpPatch("{id:guid}/progress")]
    public async Task<ActionResult<Result<EnrollmentDto>>> UpdateProgress(Guid id, UpdateEnrollmentProgressDto dto)
    {
        var result = await enrollmentService.UpdateProgressAsync(UserId, id, dto);
        return Ok(result);
    }

    [Authorize(Roles = UserRoles.Student)]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<Result>> Cancel(Guid id)
    {
        var result = await enrollmentService.CancelAsync(UserId, id);
        return Ok(result);
    }
}
