using MediatR;
using VerticalSliceTemplate.Features.Shared.Abstractions;

namespace VerticalSliceTemplate.Features.Auth.Login;

public class LoginEndpoint : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/login", async (LoginCommand command, ISender sender) =>
            {
                var result = await sender.Send(command);
                return Results.Ok(result);
            })
            .WithName("Login")
            .WithTags("Auth")
            // Unlike every other endpoint in this app, login must be reachable
            // without a token — you need to log in *to get* one.
            .AllowAnonymous()
            .Produces<LoginResponse>()
            .Produces(StatusCodes.Status401Unauthorized);
    }
}
