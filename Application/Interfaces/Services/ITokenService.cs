using Domain.Entities;

namespace Application.Interfaces.Services;

public interface ITokenService
{
    Task<(string Token, DateTime ExpiresAt)> CreateTokenAsync(ApplicationUser user);
}
