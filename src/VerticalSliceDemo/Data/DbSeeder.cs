using Microsoft.EntityFrameworkCore;
using VerticalSliceDemo.Data.Entities;
using VerticalSliceDemo.Features.Shared.Security;

namespace VerticalSliceDemo.Data;

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
