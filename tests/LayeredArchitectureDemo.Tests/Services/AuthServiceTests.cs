using LayeredArchitectureDemo.Common;
using LayeredArchitectureDemo.Models.Dtos;
using LayeredArchitectureDemo.Models.Entities;
using LayeredArchitectureDemo.Repositories;
using LayeredArchitectureDemo.Services;
using LayeredArchitectureDemo.Tests.Common;
using LayeredArchitectureDemo.Validators;
using Microsoft.Extensions.Options;
using Xunit;

namespace LayeredArchitectureDemo.Tests.Services;

public class AuthServiceTests
{
    private static IJwtTokenGenerator CreateTokenGenerator() =>
        new JwtTokenGenerator(Options.Create(new JwtOptions
        {
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            SigningKey = "test-signing-key-at-least-32-bytes-long!!",
            ExpiryMinutes = 60,
        }));

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsToken()
    {
        using var db = TestDbContextFactory.Create();
        var (hash, salt) = PasswordHasher.Hash("Passw0rd!");
        db.Users.Add(new User { Username = "admin", PasswordHash = hash, PasswordSalt = salt, Role = Roles.Admin });
        await db.SaveChangesAsync();

        var service = new AuthService(new UserRepository(db), CreateTokenGenerator(), new LoginRequestValidator());
        var result = await service.LoginAsync(new LoginRequest("admin", "Passw0rd!"), CancellationToken.None);

        Assert.Equal("admin", result.Username);
        Assert.Equal(Roles.Admin, result.Role);
        Assert.False(string.IsNullOrWhiteSpace(result.Token));
    }

    [Fact]
    public async Task LoginAsync_InvalidPassword_ThrowsUnauthorizedException()
    {
        using var db = TestDbContextFactory.Create();
        var (hash, salt) = PasswordHasher.Hash("Passw0rd!");
        db.Users.Add(new User { Username = "admin", PasswordHash = hash, PasswordSalt = salt });
        await db.SaveChangesAsync();

        var service = new AuthService(new UserRepository(db), CreateTokenGenerator(), new LoginRequestValidator());

        await Assert.ThrowsAsync<UnauthorizedException>(
            () => service.LoginAsync(new LoginRequest("admin", "wrong"), CancellationToken.None));
    }

    [Fact]
    public async Task LoginAsync_UnknownUser_ThrowsUnauthorizedException()
    {
        using var db = TestDbContextFactory.Create();
        var service = new AuthService(new UserRepository(db), CreateTokenGenerator(), new LoginRequestValidator());

        await Assert.ThrowsAsync<UnauthorizedException>(
            () => service.LoginAsync(new LoginRequest("nobody", "whatever"), CancellationToken.None));
    }
}
