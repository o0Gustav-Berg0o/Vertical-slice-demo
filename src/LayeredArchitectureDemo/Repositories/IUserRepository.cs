using LayeredArchitectureDemo.Models.Entities;

namespace LayeredArchitectureDemo.Repositories;

public interface IUserRepository
{
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken);
}
