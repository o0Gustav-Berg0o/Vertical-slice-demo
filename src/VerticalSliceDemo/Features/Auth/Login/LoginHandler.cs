using MediatR;
using Microsoft.EntityFrameworkCore;
using VerticalSliceDemo.Data;
using VerticalSliceDemo.Features.Shared.Abstractions;
using VerticalSliceDemo.Features.Shared.Security;

namespace VerticalSliceDemo.Features.Auth.Login;

public class LoginHandler(AppDbContext db, IJwtTokenGenerator tokenGenerator) : IRequestHandler<LoginCommand, LoginResponse>
{
    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Username == request.Username, cancellationToken);
        if (user is null || !PasswordHasher.Verify(request.Password, user.PasswordHash, user.PasswordSalt))
        {
            throw new UnauthorizedException("Invalid username or password.");
        }

        var (token, expiresAtUtc) = tokenGenerator.GenerateToken(user);
        return new LoginResponse(token, expiresAtUtc, user.Username);
    }
}
