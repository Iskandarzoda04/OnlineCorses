using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.Common;
using Application.DTOs.Users;
using Application.Interfaces.Services;
using Domain.Constants;

namespace WebApi.Controllers;

[Authorize(Roles = UserRoles.Admin)]
[Route("api/users")]
public class UsersController(IUserService userService) : BaseController
{
    [HttpGet]
    public async Task<ActionResult<Result<IReadOnlyList<UserSummaryDto>>>> GetAll()
    {
        var result = await userService.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Result<UserSummaryDto>>> GetById(string id)
    {
        var result = await userService.GetByIdAsync(id);
        return Ok(result);
    }

    [HttpPut("{id}/role")]
    public async Task<ActionResult<Result<UserSummaryDto>>> ChangeRole(string id, ChangeUserRoleDto dto)
    {
        var result = await userService.ChangeRoleAsync(id, dto);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<Result>> Delete(string id)
    {
        var result = await userService.DeleteAsync(id);
        return Ok(result);
    }
}
