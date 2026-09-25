using FastEndpoints;
using Microsoft.AspNetCore.Identity;
using VerticalSliceTemplate.Data.Entities;
using VerticalSliceTemplate.Features.Auth.Login;
using VerticalSliceTemplate.Features.Shared.Abstractions;
using VerticalSliceTemplate.Features.Shared.Security;
using VerticalSliceTemplate.Tests.Common;
using Xunit;

namespace VerticalSliceTemplate.Tests.Features.Auth;

public class LoginEndpointTests
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
    public async Task HandleAsync_ValidCredentials_ReturnsTokenWithRoles()
    {
        using var db = TestDbContextFactory.Create();
        var (users, signIn, roles) = IdentityTestFactory.Create(db);
        await roles.CreateAsync(new IdentityRole(Roles.Admin));
        var user = new ApplicationUser { UserName = "admin", Email = "admin@example.com" };
        await users.CreateAsync(user, "Passw0rd!");
        await users.AddToRoleAsync(user, Roles.Admin);

        var ep = Factory.Create<LoginEndpoint>(signIn, users, CreateTokenGenerator());
        await ep.HandleAsync(new LoginRequest("admin", "Passw0rd!"), CancellationToken.None);

        Assert.Equal("admin", ep.Response.Username);
        Assert.Contains(Roles.Admin, ep.Response.Roles);
        Assert.False(string.IsNullOrWhiteSpace(ep.Response.Token));
    }

    [Fact]
    public async Task HandleAsync_InvalidPassword_ThrowsUnauthorizedException()
    {
        using var db = TestDbContextFactory.Create();
        var (users, signIn, _) = IdentityTestFactory.Create(db);
        await users.CreateAsync(new ApplicationUser { UserName = "admin", Email = "admin@example.com" }, "Passw0rd!");

        var ep = Factory.Create<LoginEndpoint>(signIn, users, CreateTokenGenerator());

        await Assert.ThrowsAsync<UnauthorizedException>(
            () => ep.HandleAsync(new LoginRequest("admin", "wrong"), CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_UnknownUser_ThrowsUnauthorizedException()
    {
        using var db = TestDbContextFactory.Create();
        var (users, signIn, _) = IdentityTestFactory.Create(db);

        var ep = Factory.Create<LoginEndpoint>(signIn, users, CreateTokenGenerator());

        await Assert.ThrowsAsync<UnauthorizedException>(
            () => ep.HandleAsync(new LoginRequest("nobody", "whatever"), CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_UserWithNoRole_ReturnsEmptyRoles()
    {
        using var db = TestDbContextFactory.Create();
        var (users, signIn, _) = IdentityTestFactory.Create(db);
        await users.CreateAsync(new ApplicationUser { UserName = "user", Email = "user@example.com" }, "Passw0rd!");

        var ep = Factory.Create<LoginEndpoint>(signIn, users, CreateTokenGenerator());
        await ep.HandleAsync(new LoginRequest("user", "Passw0rd!"), CancellationToken.None);

        Assert.Empty(ep.Response.Roles);
    }
}
