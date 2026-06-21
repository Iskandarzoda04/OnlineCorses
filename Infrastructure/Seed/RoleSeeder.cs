using Domain.Constants;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Seed;

public class RoleSeeder
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AppDbContext _db;
    private readonly IConfiguration _configuration;

    public RoleSeeder(
        RoleManager<IdentityRole> roleManager,
        UserManager<ApplicationUser> userManager,
        AppDbContext db,
        IConfiguration configuration)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _db = db;
        _configuration = configuration;
    }

    public async Task SeedAsync()
    {
        var roles = new[]
        {
            UserRoles.Admin,
            UserRoles.Instructor,
            UserRoles.Student
        };

        foreach (var role in roles)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        if (!await _db.Set<Category>().AnyAsync())
        {
            _db.Set<Category>().AddRange(
                new Category { Name = "Programming", Description = "Software development courses" },
                new Category { Name = "Design", Description = "Design and product courses" },
                new Category { Name = "Business", Description = "Business and management courses" });

            await _db.SaveChangesAsync();
        }

        var adminEmail = _configuration["DefaultAdmin:Email"] ?? "admin@example.com";
        var adminPassword = _configuration["DefaultAdmin:Password"] ?? "Admin123!";

        var admin = await _userManager.FindByEmailAsync(adminEmail);
        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "Default Admin",
                EmailConfirmed = true
            };

            await _userManager.CreateAsync(admin, adminPassword);
        }

        if (!await _userManager.IsInRoleAsync(admin, UserRoles.Admin))
        {
            await _userManager.AddToRoleAsync(admin, UserRoles.Admin);
        }
    }
}
