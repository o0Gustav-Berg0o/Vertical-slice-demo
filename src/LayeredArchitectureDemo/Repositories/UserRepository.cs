using LayeredArchitectureDemo.Data;
using LayeredArchitectureDemo.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace LayeredArchitectureDemo.Repositories;

public class UserRepository(AppDbContext db) : IUserRepository
{
    public Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken) =>
        db.Users.FirstOrDefaultAsync(u => u.Username == username, cancellationToken);
}
