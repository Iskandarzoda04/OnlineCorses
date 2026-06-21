using Application.Common;
using Application.DTOs.Users;

namespace Application.Interfaces.Services;

public interface IUserService
{
    Task<Result<IReadOnlyList<UserSummaryDto>>> GetAllAsync();
    Task<Result<UserSummaryDto>> GetByIdAsync(string id);
    Task<Result<UserSummaryDto>> ChangeRoleAsync(string id, ChangeUserRoleDto dto);
    Task<Result> DeleteAsync(string id);
}
