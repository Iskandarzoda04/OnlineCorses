using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.Common;
using Application.DTOs.Auth;
using Application.Interfaces.Services;

namespace WebApi.Controllers;

[Route("api/auth")]
public class AuthController(IAuthService authService) : BaseController
{
    [HttpPost("register")]
    public async Task<ActionResult<Result<AuthResponseDto>>> Register(RegisterDto dto)
    {
        var result = await authService.RegisterAsync(dto);
        if (result.IsSuccess)
            return Ok(result);

        return result.ErrorType switch
        {
            ErrorType.Validation => BadRequest(result),
            ErrorType.Conflict => Conflict(result),
            _ => StatusCode(500, result)
        };
    }

    [HttpPost("login")]
    public async Task<ActionResult<Result<AuthResponseDto>>> Login(LoginDto dto)
    {
        var result = await authService.LoginAsync(dto);
        if (result.IsSuccess)
            return Ok(result);

        return result.ErrorType switch
        {
            ErrorType.Validation => BadRequest(result),
            ErrorType.Unauthorized => Unauthorized(result),
            _ => StatusCode(500, result)
        };
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<ActionResult<Result>> ChangePassword(ChangePasswordDto dto)
    {
        var result = await authService.ChangePasswordAsync(UserId, dto);
        if (result.IsSuccess)
            return Ok(result);

        return result.ErrorType switch
        {
            ErrorType.Validation => BadRequest(result),
            ErrorType.Unauthorized => Unauthorized(result),
            ErrorType.NotFound => NotFound(result),
            _ => StatusCode(500, result)
        };
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<Result<UserDto>>> Me()
    {
        var result = await authService.GetMeAsync(UserId);
        if (result.IsSuccess)
            return Ok(result);

        return result.ErrorType switch
        {
            ErrorType.Unauthorized => Unauthorized(result),
            ErrorType.NotFound => NotFound(result),
            _ => StatusCode(500, result)
        };
    }

    [HttpPost("forgot-password")]
    public async Task<ActionResult<Result>> ForgotPassword(ForgotPasswordDto dto)
    {
        var result = await authService.ForgotPasswordAsync(dto);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPost("reset-password")]
    public async Task<ActionResult<Result>> ResetPassword(ResetPasswordDto dto)
    {
        var result = await authService.ResetPasswordAsync(dto);
        if (result.IsSuccess)
            return Ok(result);

        return result.ErrorType switch
        {
            ErrorType.Validation => BadRequest(result),
            ErrorType.NotFound => NotFound(result),
            _ => StatusCode(500, result)
        };
    }
}
