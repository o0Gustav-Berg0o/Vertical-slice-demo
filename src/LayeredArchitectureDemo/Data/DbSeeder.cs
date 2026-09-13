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

        var (hash, salt) = PasswordHasher.Hash("Passw0rd!");
        db.Users.Add(new User { Username = "admin", PasswordHash = hash, PasswordSalt = salt });
        await db.SaveChangesAsync(cancellationToken);
    }
}
