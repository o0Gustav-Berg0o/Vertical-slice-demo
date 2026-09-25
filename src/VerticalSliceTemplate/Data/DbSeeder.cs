using Microsoft.AspNetCore.Identity;
using VerticalSliceTemplate.Data.Entities;

namespace VerticalSliceTemplate.Data;

// Seeds two demo accounts on startup so there's something to log in with
// immediately, without registering first: an Admin (can create products) and
// a plain User (cannot — a good way to see the 403 from RequireRole in action).
public static class DbSeeder
{
    public static async Task SeedAsync(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
    {
        if (!await roleManager.RoleExistsAsync(Roles.Admin))
        {
            await roleManager.CreateAsync(new IdentityRole(Roles.Admin));
        }

        await SeedUserAsync(userManager, "admin", "admin@example.com", Roles.Admin);
        await SeedUserAsync(userManager, "user", "user@example.com", role: null);
    }

    private static async Task SeedUserAsync(
        UserManager<ApplicationUser> userManager,
        string username,
        string email,
        string? role)
    {
        if (await userManager.FindByNameAsync(username) is not null)
        {
            return;
        }

        var user = new ApplicationUser { UserName = username, Email = email, EmailConfirmed = true };
        var result = await userManager.CreateAsync(user, "Passw0rd!");
        if (result.Succeeded && role is not null)
        {
            await userManager.AddToRoleAsync(user, role);
        }
    }
}
