using Microsoft.AspNetCore.Identity;
using VerticalSliceTemplate.Data.Entities;
using VerticalSliceTemplate.Features.Auth.Login;
using VerticalSliceTemplate.Features.Shared.Abstractions;
using VerticalSliceTemplate.Features.Shared.Security;
using VerticalSliceTemplate.Tests.Common;
using Xunit;

namespace VerticalSliceTemplate.Tests.Features.Auth;

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
    public async Task Handle_ValidCredentials_ReturnsTokenWithRoles()
    {
        using var db = TestDbContextFactory.Create();
        var (users, signIn, roles) = IdentityTestFactory.Create(db);
        await roles.CreateAsync(new IdentityRole(Roles.Admin));
        var user = new ApplicationUser { UserName = "admin", Email = "admin@example.com" };
        await users.CreateAsync(user, "Passw0rd!");
        await users.AddToRoleAsync(user, Roles.Admin);

        var handler = new LoginHandler(signIn, users, CreateTokenGenerator());
        var result = await handler.Handle(new LoginCommand("admin", "Passw0rd!"), CancellationToken.None);

        Assert.Equal("admin", result.Username);
        Assert.Contains(Roles.Admin, result.Roles);
        Assert.False(string.IsNullOrWhiteSpace(result.Token));
    }

    [Fact]
    public async Task Handle_InvalidPassword_ThrowsUnauthorizedException()
    {
        using var db = TestDbContextFactory.Create();
        var (users, signIn, _) = IdentityTestFactory.Create(db);
        await users.CreateAsync(new ApplicationUser { UserName = "admin", Email = "admin@example.com" }, "Passw0rd!");

        var handler = new LoginHandler(signIn, users, CreateTokenGenerator());

        await Assert.ThrowsAsync<UnauthorizedException>(
            () => handler.Handle(new LoginCommand("admin", "wrong"), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_UnknownUser_ThrowsUnauthorizedException()
    {
        using var db = TestDbContextFactory.Create();
        var (users, signIn, _) = IdentityTestFactory.Create(db);

        var handler = new LoginHandler(signIn, users, CreateTokenGenerator());

        await Assert.ThrowsAsync<UnauthorizedException>(
            () => handler.Handle(new LoginCommand("nobody", "whatever"), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_UserWithNoRole_ReturnsEmptyRoles()
    {
        using var db = TestDbContextFactory.Create();
        var (users, signIn, _) = IdentityTestFactory.Create(db);
        await users.CreateAsync(new ApplicationUser { UserName = "user", Email = "user@example.com" }, "Passw0rd!");

        var handler = new LoginHandler(signIn, users, CreateTokenGenerator());
        var result = await handler.Handle(new LoginCommand("user", "Passw0rd!"), CancellationToken.None);

        Assert.Empty(result.Roles);
    }
}
