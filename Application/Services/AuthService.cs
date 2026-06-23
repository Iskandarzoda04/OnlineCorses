using Application.Common;
using Application.DTOs.Auth;
using Application.DTOs.Email;
using Application.Interfaces.Services;
using Domain.Constants;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Application.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        ITokenService tokenService,
        IEmailService emailService,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<Result<AuthResponseDto>> RegisterAsync(RegisterDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.FullName))
            return Result<AuthResponseDto>.Failure("Full name is required.", ErrorType.Validation);

        if (string.IsNullOrWhiteSpace(dto.Email))
            return Result<AuthResponseDto>.Failure("Email is required.", ErrorType.Validation);

        if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password.Length < 6)
            return Result<AuthResponseDto>.Failure("Password must be at least 6 characters.", ErrorType.Validation);

        if (dto.Password != dto.ConfirmPassword)
            return Result<AuthResponseDto>.Failure("Passwords do not match.", ErrorType.Validation);

        var role = string.IsNullOrWhiteSpace(dto.Role) ? UserRoles.Student : dto.Role;
        if (role is not (UserRoles.Student or UserRoles.Instructor))
            return Result<AuthResponseDto>.Failure("Role must be Student or Instructor.", ErrorType.Validation);

        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser is not null)
            return Result<AuthResponseDto>.Failure("Email already exists.", ErrorType.Conflict);

        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            FullName = dto.FullName
        };

        var createResult = await _userManager.CreateAsync(user, dto.Password);
        if (!createResult.Succeeded)
            return Result<AuthResponseDto>.Failure("User registration failed.", ErrorType.Validation);

        var addToRoleResult = await _userManager.AddToRoleAsync(user, role);
        if (!addToRoleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);
            return Result<AuthResponseDto>.Failure("Failed to assign user role.", ErrorType.Validation);
        }

        _logger.LogInformation("User {Email} registered as {Role}", dto.Email, role);

        var token = await _tokenService.CreateTokenAsync(user);
        var roles = await _userManager.GetRolesAsync(user);

        return Result<AuthResponseDto>.Success(new AuthResponseDto
        {
            Token = token.Token,
            ExpiresAt = token.ExpiresAt,
            User = new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Roles = roles.ToList()
            }
        });
    }

    public async Task<Result<AuthResponseDto>> LoginAsync(LoginDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
            return Result<AuthResponseDto>.Failure("Email and password are required.", ErrorType.Validation);

        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user is null)
            return Result<AuthResponseDto>.Failure("Invalid email or password.", ErrorType.Unauthorized);

        if (await _userManager.IsLockedOutAsync(user))
            return Result<AuthResponseDto>.Failure("User is locked out. Try again later.", ErrorType.Unauthorized);

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
        if (!isPasswordValid)
        {
            await _userManager.AccessFailedAsync(user);
            return Result<AuthResponseDto>.Failure("Invalid email or password.", ErrorType.Unauthorized);
        }

        await _userManager.ResetAccessFailedCountAsync(user);

        var token = await _tokenService.CreateTokenAsync(user);
        var roles = await _userManager.GetRolesAsync(user);

        return Result<AuthResponseDto>.Success(new AuthResponseDto
        {
            Token = token.Token,
            ExpiresAt = token.ExpiresAt,
            User = new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                Roles = roles.ToList()
            }
        });
    }

    public async Task<Result> ChangePasswordAsync(string userId, ChangePasswordDto dto)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return Result.Failure("User is not authorized.", ErrorType.Unauthorized);

        if (string.IsNullOrWhiteSpace(dto.CurrentPassword))
            return Result.Failure("Current password is required.", ErrorType.Validation);

        if (string.IsNullOrWhiteSpace(dto.NewPassword) || dto.NewPassword.Length < 6)
            return Result.Failure("New password must be at least 6 characters.", ErrorType.Validation);

        if (dto.NewPassword != dto.ConfirmNewPassword)
            return Result.Failure("Passwords do not match.", ErrorType.Validation);

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return Result.Failure("User not found.", ErrorType.NotFound);

        var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
        if (!result.Succeeded)
            return Result.Failure("Password change failed.", ErrorType.Validation);

        return Result.Success("Password changed successfully.");
    }

    public async Task<Result<UserDto>> GetMeAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return Result<UserDto>.Failure("User is not authorized.", ErrorType.Unauthorized);

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return Result<UserDto>.Failure("User not found.", ErrorType.NotFound);

        var roles = await _userManager.GetRolesAsync(user);

        return Result<UserDto>.Success(new UserDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            Roles = roles.ToList()
        });
    }

    public async Task<Result> ForgotPasswordAsync(ForgotPasswordDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email))
            return Result.Failure("Email is required.", ErrorType.Validation);

        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user is not null)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(token)).TrimEnd('=').Replace('+', '-').Replace('/', '_');
            var encodedEmail = Convert.ToBase64String(Encoding.UTF8.GetBytes(user.Email ?? dto.Email)).TrimEnd('=').Replace('+', '-').Replace('/', '_');
            var frontendBaseUrl = Environment.GetEnvironmentVariable("FRONTEND_BASE_URL") ?? "http://localhost:3000";
            var resetLink = $"{frontendBaseUrl.TrimEnd('/')}/reset-password?email={encodedEmail}&token={encodedToken}";

            await _emailService.SendAsync(new EmailMessageDto
            {
                To = user.Email ?? dto.Email,
                Subject = "Reset password - Online Courses",
                Body = $"""
                    <h2>Reset password</h2>
                    <p>You requested a password reset.</p>
                    <p>Click the link below to reset your password:</p>
                    <p><a href="{resetLink}">Reset password</a></p>
                    <p>If you did not request this, ignore this email.</p>
                    """,
                IsHtml = true
            });
        }

        return Result.Success("If the email exists, reset instructions have been sent.");
    }

    public async Task<Result> ResetPasswordAsync(ResetPasswordDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Token))
            return Result.Failure("Email and token are required.", ErrorType.Validation);

        if (string.IsNullOrWhiteSpace(dto.NewPassword) || dto.NewPassword.Length < 6)
            return Result.Failure("New password must be at least 6 characters.", ErrorType.Validation);

        if (dto.NewPassword != dto.ConfirmPassword)
            return Result.Failure("Passwords do not match.", ErrorType.Validation);

        string email;
        string token;

        try
        {
            var emailBase64 = dto.Email.Replace('-', '+').Replace('_', '/');
            var tokenBase64 = dto.Token.Replace('-', '+').Replace('_', '/');

            emailBase64 = emailBase64.PadRight(emailBase64.Length + (4 - emailBase64.Length % 4) % 4, '=');
            tokenBase64 = tokenBase64.PadRight(tokenBase64.Length + (4 - tokenBase64.Length % 4) % 4, '=');

            email = Encoding.UTF8.GetString(Convert.FromBase64String(emailBase64));
            token = Encoding.UTF8.GetString(Convert.FromBase64String(tokenBase64));
        }
        catch
        {
            email = dto.Email;
            token = dto.Token;
        }

        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
            return Result.Failure("Invalid reset request.", ErrorType.Validation);

        var result = await _userManager.ResetPasswordAsync(user, token, dto.NewPassword);
        if (!result.Succeeded)
            return Result.Failure("Password reset failed.", ErrorType.Validation);

        return Result.Success("Password reset successfully.");
    }

}
