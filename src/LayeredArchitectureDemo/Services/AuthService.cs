using FluentValidation;
using LayeredArchitectureDemo.Common;
using LayeredArchitectureDemo.Models.Dtos;
using LayeredArchitectureDemo.Repositories;

namespace LayeredArchitectureDemo.Services;

public class AuthService(
    IUserRepository userRepository,
    IJwtTokenGenerator tokenGenerator,
    IValidator<LoginRequest> validator) : IAuthService
{
    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var user = await userRepository.GetByUsernameAsync(request.Username, cancellationToken);
        if (user is null || !PasswordHasher.Verify(request.Password, user.PasswordHash, user.PasswordSalt))
        {
            throw new UnauthorizedException("Invalid username or password.");
        }

        var (token, expiresAtUtc) = tokenGenerator.GenerateToken(user);
        return new LoginResponse(token, expiresAtUtc, user.Username, user.Role);
    }
}
