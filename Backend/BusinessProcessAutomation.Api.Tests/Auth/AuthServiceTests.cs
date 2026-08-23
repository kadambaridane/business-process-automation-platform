using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using BusinessProcessAutomation.Api.Data;
using BusinessProcessAutomation.Api.DTOs.Auth;
using BusinessProcessAutomation.Api.Helpers;
using BusinessProcessAutomation.Api.Models;
using BusinessProcessAutomation.Api.Services;
using Xunit;

namespace BusinessProcessAutomation.Api.Tests.Auth;

public class AuthServiceTests
{
    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static AuthService CreateAuthService(AppDbContext dbContext)
    {
        var passwordHasher = new PasswordHasher();
        var jwtSettings = Options.Create(new JwtSettings
        {
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            Key = "unit-test-signing-key-at-least-32-characters-long",
            ExpiryMinutes = 60
        });
        var tokenGenerator = new JwtTokenGenerator(jwtSettings);
        return new AuthService(dbContext, passwordHasher, tokenGenerator);
    }

    private static async Task<User> SeedUserAsync(AppDbContext dbContext, string password, bool isActive = true, string roleName = "Admin")
    {
        var passwordHasher = new PasswordHasher();
        var role = new Role { Name = roleName };
        dbContext.Roles.Add(role);

        var user = new User
        {
            FullName = "Test User",
            Email = "test.user@example.com",
            Username = "testuser",
            PasswordHash = passwordHasher.Hash(password),
            IsActive = isActive
        };
        user.UserRoles.Add(new UserRole { Role = role });
        dbContext.Users.Add(user);

        await dbContext.SaveChangesAsync();
        return user;
    }

    [Fact]
    public async Task LoginAsync_WithCorrectCredentials_ReturnsTokenAndUser()
    {
        var dbContext = CreateDbContext();
        await SeedUserAsync(dbContext, "P@ssw0rd123");
        var authService = CreateAuthService(dbContext);

        var result = await authService.LoginAsync(new LoginRequestDto { UsernameOrEmail = "testuser", Password = "P@ssw0rd123" });

        Assert.False(string.IsNullOrWhiteSpace(result.Token));
        Assert.Equal("testuser", result.User.Username);
        Assert.Contains("Admin", result.User.Roles);
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ThrowsUnauthorized()
    {
        var dbContext = CreateDbContext();
        await SeedUserAsync(dbContext, "P@ssw0rd123");
        var authService = CreateAuthService(dbContext);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            authService.LoginAsync(new LoginRequestDto { UsernameOrEmail = "testuser", Password = "WrongPassword" }));
    }

    [Fact]
    public async Task LoginAsync_WithUnknownUser_ThrowsUnauthorized()
    {
        var dbContext = CreateDbContext();
        var authService = CreateAuthService(dbContext);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            authService.LoginAsync(new LoginRequestDto { UsernameOrEmail = "nobody", Password = "whatever" }));
    }

    [Fact]
    public async Task LoginAsync_WithDeactivatedUser_ThrowsUnauthorized()
    {
        var dbContext = CreateDbContext();
        await SeedUserAsync(dbContext, "P@ssw0rd123", isActive: false);
        var authService = CreateAuthService(dbContext);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            authService.LoginAsync(new LoginRequestDto { UsernameOrEmail = "testuser", Password = "P@ssw0rd123" }));
    }

    [Fact]
    public async Task LoginAsync_ByEmail_Succeeds()
    {
        var dbContext = CreateDbContext();
        await SeedUserAsync(dbContext, "P@ssw0rd123");
        var authService = CreateAuthService(dbContext);

        var result = await authService.LoginAsync(new LoginRequestDto { UsernameOrEmail = "test.user@example.com", Password = "P@ssw0rd123" });

        Assert.Equal("testuser", result.User.Username);
    }
}
