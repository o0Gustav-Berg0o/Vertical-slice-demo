using FluentValidation;
using VerticalSliceTemplate.Features.Auth.Register;
using VerticalSliceTemplate.Tests.Common;
using Xunit;

namespace VerticalSliceTemplate.Tests.Features.Auth;

public class RegisterHandlerTests
{
    [Fact]
    public async Task Handle_ValidRequest_CreatesUser()
    {
        using var db = TestDbContextFactory.Create();
        var (users, _, _) = IdentityTestFactory.Create(db);

        var handler = new RegisterHandler(users);
        var result = await handler.Handle(
            new RegisterCommand("jane", "jane@example.com", "Passw0rd!"),
            CancellationToken.None);

        Assert.Equal("jane", result.Username);
        Assert.NotNull(await users.FindByNameAsync("jane"));
    }

    [Fact]
    public async Task Handle_DuplicateUsername_ThrowsValidationException()
    {
        using var db = TestDbContextFactory.Create();
        var (users, _, _) = IdentityTestFactory.Create(db);
        var handler = new RegisterHandler(users);
        await handler.Handle(new RegisterCommand("jane", "jane@example.com", "Passw0rd!"), CancellationToken.None);

        await Assert.ThrowsAsync<ValidationException>(
            () => handler.Handle(new RegisterCommand("jane", "other@example.com", "Passw0rd!"), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WeakPassword_ThrowsValidationException()
    {
        using var db = TestDbContextFactory.Create();
        var (users, _, _) = IdentityTestFactory.Create(db);
        var handler = new RegisterHandler(users);

        await Assert.ThrowsAsync<ValidationException>(
            () => handler.Handle(new RegisterCommand("jane", "jane@example.com", "weak"), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_NewUser_GetsNoRole()
    {
        using var db = TestDbContextFactory.Create();
        var (users, _, _) = IdentityTestFactory.Create(db);
        var handler = new RegisterHandler(users);

        await handler.Handle(new RegisterCommand("jane", "jane@example.com", "Passw0rd!"), CancellationToken.None);

        var user = await users.FindByNameAsync("jane");
        var roles = await users.GetRolesAsync(user!);
        Assert.Empty(roles);
    }
}
