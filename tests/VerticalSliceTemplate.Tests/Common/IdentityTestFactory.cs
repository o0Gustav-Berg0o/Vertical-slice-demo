using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using VerticalSliceTemplate.Data;
using VerticalSliceTemplate.Data.Entities;

namespace VerticalSliceTemplate.Tests.Common;

// UserManager/SignInManager/RoleManager have large constructors that are
// impractical to mock directly with something like Moq. Instead, this spins up
// a small real DI container with AddIdentityCore wired to an EF Core InMemory
// AppDbContext (see TestDbContextFactory) and resolves real manager instances
// from it — the commonly recommended way to unit-test Identity-dependent code.
public static class IdentityTestFactory
{
    public static (UserManager<ApplicationUser> Users, SignInManager<ApplicationUser> SignIn, RoleManager<IdentityRole> Roles) Create(AppDbContext db)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDataProtection();
        services.AddAuthentication();
        services.AddSingleton(db);
        services.AddHttpContextAccessor();
        services.AddIdentityCore<ApplicationUser>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        var provider = services.BuildServiceProvider();
        return (
            provider.GetRequiredService<UserManager<ApplicationUser>>(),
            provider.GetRequiredService<SignInManager<ApplicationUser>>(),
            provider.GetRequiredService<RoleManager<IdentityRole>>());
    }
}
