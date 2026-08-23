using Microsoft.EntityFrameworkCore;
using BusinessProcessAutomation.Api.Interfaces;
using BusinessProcessAutomation.Api.Models;

namespace BusinessProcessAutomation.Api.Data;

public static class DbSeeder
{
    public static async Task SeedDefaultAdminAsync(AppDbContext dbContext, IPasswordHasher passwordHasher)
    {
        if (await dbContext.Users.AnyAsync())
        {
            return;
        }

        var adminRole = await dbContext.Roles.FirstAsync(r => r.Name == "Admin");

        var admin = new User
        {
            FullName = "System Administrator",
            Email = "admin@bpap.local",
            Username = "admin",
            PasswordHash = passwordHasher.Hash("Admin@12345"),
            IsActive = true
        };

        admin.UserRoles.Add(new UserRole { Role = adminRole });

        dbContext.Users.Add(admin);
        await dbContext.SaveChangesAsync();
    }
}
