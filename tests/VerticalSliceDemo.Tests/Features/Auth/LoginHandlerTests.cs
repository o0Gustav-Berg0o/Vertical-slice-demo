using VerticalSliceDemo.Data.Entities;
using VerticalSliceDemo.Features.Auth.Login;
using VerticalSliceDemo.Features.Shared.Abstractions;
using VerticalSliceDemo.Features.Shared.Security;
using VerticalSliceDemo.Tests.Common;
using Xunit;

namespace VerticalSliceDemo.Tests.Features.Auth;

public class LoginHandlerTests
{
    private static IJwtTokenGenerator CreateTokenGenerator() =>
        new JwtTokenGenerator(Microsoft.Extensions.Options.Options.Create(new JwtOptions
        {
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            SigningKey = "test-signing-key-at-least-32-bytes-long!!",
            ExpiryMinutes = 60,
        }));

    [Fact]
    public async Task Handle_ValidCredentials_ReturnsToken()
    {
        using var db = TestDbContextFactory.Create();
        var (hash, salt) = PasswordHasher.Hash("Passw0rd!");
        db.Users.Add(new User { Username = "admin", PasswordHash = hash, PasswordSalt = salt, Role = Roles.Admin });
        await db.SaveChangesAsync();

        var handler = new LoginHandler(db, CreateTokenGenerator());
        var result = await handler.Handle(new LoginCommand("admin", "Passw0rd!"), CancellationToken.None);

        Assert.Equal("admin", result.Username);
        Assert.Equal(Roles.Admin, result.Role);
        Assert.False(string.IsNullOrWhiteSpace(result.Token));
    }

    [Fact]
    public async Task Handle_InvalidPassword_ThrowsUnauthorizedException()
    {
        using var db = TestDbContextFactory.Create();
        var (hash, salt) = PasswordHasher.Hash("Passw0rd!");
        db.Users.Add(new User { Username = "admin", PasswordHash = hash, PasswordSalt = salt });
        await db.SaveChangesAsync();

        var handler = new LoginHandler(db, CreateTokenGenerator());

        await Assert.ThrowsAsync<UnauthorizedException>(
            () => handler.Handle(new LoginCommand("admin", "wrong"), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_UnknownUser_ThrowsUnauthorizedException()
    {
        using var db = TestDbContextFactory.Create();
        var handler = new LoginHandler(db, CreateTokenGenerator());

        await Assert.ThrowsAsync<UnauthorizedException>(
            () => handler.Handle(new LoginCommand("nobody", "whatever"), CancellationToken.None));
    }
}
