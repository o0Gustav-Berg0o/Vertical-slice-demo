using MediatR;
using Microsoft.AspNetCore.Identity;
using VerticalSliceTemplate.Data.Entities;
using VerticalSliceTemplate.Features.Shared.Abstractions;
using VerticalSliceTemplate.Features.Shared.Security;

namespace VerticalSliceTemplate.Features.Auth.Login;

// Uses SignInManager/UserManager instead of talking to AppDbContext directly —
// this is what "using .NET's own Identity" means in practice: user lookup,
// password verification, and lockout are all Identity's job, not ours.
public class LoginHandler(
    SignInManager<ApplicationUser> signInManager,
    UserManager<ApplicationUser> userManager,
    IJwtTokenGenerator tokenGenerator) : IRequestHandler<LoginCommand, LoginResponse>
{
    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // UserName is the login identifier here (kept for continuity with the
        // old Username field). ApplicationUser also has an Email property if
        // you'd rather let students log in with their email instead.
        var user = await userManager.FindByNameAsync(request.Username);
        if (user is null)
        {
            throw new UnauthorizedException("Invalid username or password.");
        }

        // CheckPasswordSignInAsync (not the simpler UserManager.CheckPasswordAsync)
        // because it enforces the lockout policy configured in Program.cs: too
        // many failed attempts locks the account out for a while, even if the
        // correct password is given afterwards.
        var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            throw new UnauthorizedException("Invalid username or password.");
        }

        var roles = (await userManager.GetRolesAsync(user)).ToList();
        var (token, expiresAtUtc) = tokenGenerator.GenerateToken(user, roles);
        return new LoginResponse(token, expiresAtUtc, user.UserName!, roles);
    }
}
