using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Domain.Constants;

namespace WebApi.Controllers;

[ApiController]
public abstract class BaseController : ControllerBase
{
    protected string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
    protected bool IsAdmin => User.IsInRole(UserRoles.Admin);
}
