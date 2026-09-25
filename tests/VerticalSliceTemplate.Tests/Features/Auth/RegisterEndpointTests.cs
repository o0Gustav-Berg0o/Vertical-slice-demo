using FastEndpoints;
using FluentValidation;
using VerticalSliceTemplate.Features.Auth.Register;
using VerticalSliceTemplate.Tests.Common;
using Xunit;

namespace VerticalSliceTemplate.Tests.Features.Auth;

public class RegisterEndpointTests
{
    [Fact]
    public async Task HandleAsync_ValidRequest_CreatesUser()
    {
        using var db = TestDbContextFactory.Create();
        var (users, _, _) = IdentityTestFactory.Create(db);

        var ep = Factory.Create<RegisterEndpoint>(users);
        await ep.HandleAsync(new RegisterRequest("jane", "jane@example.com", "Passw0rd!"), CancellationToken.None);

        Assert.Equal("jane", ep.Response.Username);
        Assert.NotNull(await users.FindByNameAsync("jane"));
    }

    [Fact]
    public async Task HandleAsync_DuplicateUsername_ThrowsValidationException()
    {
        using var db = TestDbContextFactory.Create();
        var (users, _, _) = IdentityTestFactory.Create(db);
        var ep = Factory.Create<RegisterEndpoint>(users);
        await ep.HandleAsync(new RegisterRequest("jane", "jane@example.com", "Passw0rd!"), CancellationToken.None);

        await Assert.ThrowsAsync<ValidationException>(
            () => ep.HandleAsync(new RegisterRequest("jane", "other@example.com", "Passw0rd!"), CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_WeakPassword_ThrowsValidationException()
    {
        using var db = TestDbContextFactory.Create();
        var (users, _, _) = IdentityTestFactory.Create(db);
        var ep = Factory.Create<RegisterEndpoint>(users);

        await Assert.ThrowsAsync<ValidationException>(
            () => ep.HandleAsync(new RegisterRequest("jane", "jane@example.com", "weak"), CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_NewUser_GetsNoRole()
    {
        using var db = TestDbContextFactory.Create();
        var (users, _, _) = IdentityTestFactory.Create(db);
        var ep = Factory.Create<RegisterEndpoint>(users);

        await ep.HandleAsync(new RegisterRequest("jane", "jane@example.com", "Passw0rd!"), CancellationToken.None);

        var user = await users.FindByNameAsync("jane");
        var roles = await users.GetRolesAsync(user!);
        Assert.Empty(roles);
    }
}
