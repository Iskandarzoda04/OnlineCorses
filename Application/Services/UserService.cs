using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Application.Common;
using Application.DTOs.Users;
using Application.Interfaces.Services;
using Domain.Constants;
using Domain.Entities;

namespace Application.Services;

public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<UserService> _logger;

    public UserService(
        UserManager<ApplicationUser> userManager,
        ILogger<UserService> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<UserSummaryDto>>> GetAllAsync()
    {
        var users = _userManager.Users.OrderBy(u => u.FullName).ToList();
        var result = new List<UserSummaryDto>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            result.Add(new UserSummaryDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                Roles = roles.ToList()
            });
        }

        _logger.LogInformation("Returned {Count} users", result.Count);
        return Result<IReadOnlyList<UserSummaryDto>>.Success(result);
    }

    public async Task<Result<UserSummaryDto>> GetByIdAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
            return Result<UserSummaryDto>.Failure("User not found.", ErrorType.NotFound);

        var roles = await _userManager.GetRolesAsync(user);
        var dto = new UserSummaryDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            Roles = roles.ToList()
        };

        return Result<UserSummaryDto>.Success(dto);
    }

    public async Task<Result<UserSummaryDto>> ChangeRoleAsync(string id, ChangeUserRoleDto dto)
    {
        if (!UserRoles.All.Contains(dto.Role))
            return Result<UserSummaryDto>.Failure("Invalid role.", ErrorType.Validation);

        var user = await _userManager.FindByIdAsync(id);
        if (user is null) return Result<UserSummaryDto>.Failure("User not found.", ErrorType.NotFound);

        var currentRoles = await _userManager.GetRolesAsync(user);
        if (currentRoles.Count > 0)
        {
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
                return Result<UserSummaryDto>.Failure(string.Join("; ", removeResult.Errors.Select(e => e.Description)), ErrorType.Validation);
        }

        var addResult = await _userManager.AddToRoleAsync(user, dto.Role);
        if (!addResult.Succeeded)
            return Result<UserSummaryDto>.Failure(string.Join("; ", addResult.Errors.Select(e => e.Description)), ErrorType.Validation);

        _logger.LogInformation("User {UserId} role changed to {Role}", id, dto.Role);
        var roles = await _userManager.GetRolesAsync(user);
        var userDto = new UserSummaryDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            Roles = roles.ToList()
        };

        return Result<UserSummaryDto>.Success(userDto);
    }

    public async Task<Result> DeleteAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null) return Result.Failure("User not found.", ErrorType.NotFound);

        var result = await _userManager.DeleteAsync(user);
        return result.Succeeded
            ? Result.Success("User deleted.")
            : Result.Failure(string.Join("; ", result.Errors.Select(e => e.Description)), ErrorType.Validation);
    }
}
