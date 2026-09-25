using FastEndpoints;
using Microsoft.AspNetCore.Identity;
using VerticalSliceTemplate.Data.Entities;
using VerticalSliceTemplate.Features.Shared.Abstractions;
using VerticalSliceTemplate.Features.Shared.Security;

namespace VerticalSliceTemplate.Features.Auth.Login;

// Uses SignInManager/UserManager instead of talking to AppDbContext directly —
// this is what "using .NET's own Identity" means in practice: user lookup,
// password verification, and lockout are all Identity's job, not this
// endpoint's.
public class LoginEndpoint(
    SignInManager<ApplicationUser> signInManager,
    UserManager<ApplicationUser> userManager,
    IJwtTokenGenerator tokenGenerator) : Endpoint<LoginRequest, LoginResponse>
{
    public override void Configure()
    {
        Post("/api/auth/login");

        // Unlike every other endpoint in this app, login must be reachable
        // without a token — you need to log in *to get* one.
        AllowAnonymous();
    }

    public override async Task HandleAsync(LoginRequest req, CancellationToken ct)
    {
        // UserName is the login identifier here (kept for continuity with the
        // old Username field). ApplicationUser also has an Email property if
        // you'd rather let students log in with their email instead.
        var user = await userManager.FindByNameAsync(req.Username);
        if (user is null)
        {
            throw new UnauthorizedException("Invalid username or password.");
        }

        // CheckPasswordSignInAsync (not the simpler UserManager.CheckPasswordAsync)
        // because it enforces the lockout policy configured in Program.cs: too
        // many failed attempts locks the account out for a while, even if the
        // correct password is given afterwards.
        var result = await signInManager.CheckPasswordSignInAsync(user, req.Password, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            throw new UnauthorizedException("Invalid username or password.");
        }

        var roles = (await userManager.GetRolesAsync(user)).ToList();
        var (token, expiresAtUtc) = tokenGenerator.GenerateToken(user, roles);

        await Send.OkAsync(new LoginResponse(token, expiresAtUtc, user.UserName!, roles), ct);
    }
}
