using LayeredArchitectureDemo.Common;
using LayeredArchitectureDemo.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace LayeredArchitectureDemo.Data;

public static class DbSeeder
{
    public static async Task SeedDemoUserAsync(AppDbContext db, CancellationToken cancellationToken = default)
    {
        if (await db.Users.AnyAsync(cancellationToken))
        {
            return;
        }

        var (adminHash, adminSalt) = PasswordHasher.Hash("Passw0rd!");
        db.Users.Add(new User { Username = "admin", PasswordHash = adminHash, PasswordSalt = adminSalt, Role = Roles.Admin });

        var (userHash, userSalt) = PasswordHasher.Hash("Passw0rd!");
        db.Users.Add(new User { Username = "user", PasswordHash = userHash, PasswordSalt = userSalt, Role = Roles.User });

        await db.SaveChangesAsync(cancellationToken);
    }
}
