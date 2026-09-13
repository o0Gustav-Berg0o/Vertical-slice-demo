using LayeredArchitectureDemo.Models.Dtos;

namespace LayeredArchitectureDemo.Services;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
}
