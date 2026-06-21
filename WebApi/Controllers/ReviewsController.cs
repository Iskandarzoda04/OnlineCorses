using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.Common;
using Application.DTOs.Reviews;
using Application.Interfaces.Services;
using Domain.Constants;

namespace WebApi.Controllers;

[Route("api/courses/{courseId:guid}/reviews")]
public class ReviewsController(IReviewService reviewService) : BaseController
{
    [HttpGet]
    public async Task<ActionResult<Result<IReadOnlyList<ReviewDto>>>> GetByCourse(Guid courseId)
    {
        var result = await reviewService.GetByCourseAsync(courseId);
        return Ok(result);
    }

    [Authorize(Roles = UserRoles.Student)]
    [HttpPost]
    public async Task<ActionResult<Result<ReviewDto>>> Create(Guid courseId, CreateReviewDto dto)
    {
        dto.CourseId = courseId;
        var result = await reviewService.CreateAsync(UserId, dto);
        return Ok(result);
    }

    [Authorize(Roles = UserRoles.Student)]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<Result<ReviewDto>>> Update(Guid id, UpdateReviewDto dto)
    {
        var result = await reviewService.UpdateAsync(id, UserId, dto);
        return Ok(result);
    }

    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<Result>> Delete(Guid id)
    {
        var result = await reviewService.DeleteAsync(id, UserId, IsAdmin);
        return Ok(result);
    }
}
