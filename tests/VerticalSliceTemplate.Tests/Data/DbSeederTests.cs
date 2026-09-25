using Microsoft.EntityFrameworkCore;
using VerticalSliceTemplate.Data;
using VerticalSliceTemplate.Data.Entities;
using VerticalSliceTemplate.Tests.Common;
using Xunit;

namespace VerticalSliceTemplate.Tests.Data;

public class DbSeederTests
{
    [Fact]
    public async Task SeedAsync_CreatesAdminRoleAndSeededUsers()
    {
        using var db = TestDbContextFactory.Create();
        var (users, _, roles) = IdentityTestFactory.Create(db);

        await DbSeeder.SeedAsync(roles, users);

        Assert.True(await roles.RoleExistsAsync(Roles.Admin));

        var admin = await users.FindByNameAsync("admin");
        Assert.NotNull(admin);
        Assert.Contains(Roles.Admin, await users.GetRolesAsync(admin!));

        var plainUser = await users.FindByNameAsync("user");
        Assert.NotNull(plainUser);
        Assert.Empty(await users.GetRolesAsync(plainUser!));
    }

    [Fact]
    public async Task SeedAsync_CalledTwice_IsIdempotent()
    {
        using var db = TestDbContextFactory.Create();
        var (users, _, roles) = IdentityTestFactory.Create(db);

        await DbSeeder.SeedAsync(roles, users);
        await DbSeeder.SeedAsync(roles, users);

        Assert.Single(await users.Users.Where(u => u.UserName == "admin").ToListAsync());
    }
}
