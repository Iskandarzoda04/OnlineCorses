using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.BackgroundServices;

public class DeleteUserBackgroundService(
    IServiceScopeFactory scopeFactory,
    ILogger<DeleteUserBackgroundService> logger) : BackgroundService
{
    private static readonly TimeSpan CheckInterval = TimeSpan.FromHours(24);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await DeleteNotConfirmedUsersAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Delete not confirmed users background job failed.");
            }

            await Task.Delay(CheckInterval, stoppingToken);
        }
    }

    public async Task DeleteNotConfirmedUsersAsync(CancellationToken cancellationToken = default)
    {
        using var scope = scopeFactory.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var users = userManager.Users
            .Where(u => !u.EmailConfirmed)
            .ToList();

        foreach (var user in users)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var result = await userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                logger.LogInformation("Deleted not confirmed user {UserId}", user.Id);
            }
            else
            {
                logger.LogWarning(
                    "Failed to delete not confirmed user {UserId}: {Errors}",
                    user.Id,
                    string.Join("; ", result.Errors.Select(e => e.Description)));
            }
        }
    }
}
