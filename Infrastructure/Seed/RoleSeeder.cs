using Domain.Constants;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Seed;

public class RoleSeeder(
    RoleManager<IdentityRole> roleManager,
    UserManager<ApplicationUser> userManager,
    AppDbContext context,
    IConfiguration configuration)
{
    public async Task SeedAsync()
    {
        await context.Database.EnsureCreatedAsync();

        var roles = new[]
        {
            UserRoles.Admin,
            UserRoles.Instructor,
            UserRoles.Student
        };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        if (!await context.Set<Category>().AnyAsync())
        {
            context.Set<Category>().AddRange(
                new Category { Name = "Programming", Description = "Software development courses" },
                new Category { Name = "Design", Description = "Design and product courses" },
                new Category { Name = "Business", Description = "Business and management courses" });

            await context.SaveChangesAsync();
        }

        var adminEmail = configuration["DefaultAdmin:Email"] ?? "admin@example.com";
        var adminPassword = configuration["DefaultAdmin:Password"] ?? "Admin123!";

        var admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "Default Admin",
                EmailConfirmed = true
            };

            await userManager.CreateAsync(admin, adminPassword);
        }

        if (!await userManager.IsInRoleAsync(admin, UserRoles.Admin))
        {
            await userManager.AddToRoleAsync(admin, UserRoles.Admin);
        }
    }
}
