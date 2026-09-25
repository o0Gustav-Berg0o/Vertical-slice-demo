using MediatR;
using VerticalSliceTemplate.Features.Shared.Abstractions;

namespace VerticalSliceTemplate.Features.Auth.Register;

public class RegisterEndpoint : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/register", async (RegisterCommand command, ISender sender) =>
            {
                var result = await sender.Send(command);
                return Results.Ok(result);
            })
            .WithName("Register")
            .WithTags("Auth")
            .AllowAnonymous()
            .Produces<RegisterResponse>()
            .ProducesValidationProblem();
    }
}
