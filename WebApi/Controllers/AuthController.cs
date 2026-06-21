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
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<ActionResult<Result<AuthResponseDto>>> Login(LoginDto dto)
    {
        var result = await authService.LoginAsync(dto);
        return Ok(result);
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<ActionResult<Result>> ChangePassword(ChangePasswordDto dto)
    {
        var result = await authService.ChangePasswordAsync(UserId, dto);
        return Ok(result);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<Result<UserDto>>> Me()
    {
        var result = await authService.GetMeAsync(UserId);
        return Ok(result);
    }

    [HttpPost("forgot-password")]
    public async Task<ActionResult<Result>> ForgotPassword(ForgotPasswordDto dto)
    {
        var result = await authService.ForgotPasswordAsync(dto);
        return Ok(result);
    }

    [HttpPost("reset-password")]
    public async Task<ActionResult<Result>> ResetPassword(ResetPasswordDto dto)
    {
        var result = await authService.ResetPasswordAsync(dto);
        return Ok(result);
    }
}
