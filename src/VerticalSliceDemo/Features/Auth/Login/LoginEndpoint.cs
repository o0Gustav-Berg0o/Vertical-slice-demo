using MediatR;
using VerticalSliceDemo.Features.Shared.Abstractions;

namespace VerticalSliceDemo.Features.Auth.Login;

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
            .AllowAnonymous()
            .Produces<LoginResponse>()
            .Produces(StatusCodes.Status401Unauthorized);
    }
}
