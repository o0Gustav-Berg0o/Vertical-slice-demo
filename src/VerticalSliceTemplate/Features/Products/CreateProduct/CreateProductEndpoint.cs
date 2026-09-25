using MediatR;
using VerticalSliceTemplate.Data.Entities;
using VerticalSliceTemplate.Features.Shared.Abstractions;

namespace VerticalSliceTemplate.Features.Products.CreateProduct;

// Implements IEndpoint so WebApplicationExtensions.MapFeatureEndpoints finds
// this class by reflection and calls MapEndpoint below — nothing to register
// in Program.cs. The handler pattern (parse input -> sender.Send(command) ->
// map the result to an HTTP response) is identical across every endpoint in
// this app; only the route, the command type, and the success response shape
// change.
public class CreateProductEndpoint : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/products", async (CreateProductCommand command, ISender sender) =>
            {
                var result = await sender.Send(command);
                return Results.Created($"/api/products/{result.Id}", result);
            })
            .WithName("CreateProduct")
            .WithTags("Products")
            // The one endpoint in this template that needs more than "logged in":
            // RequireRole reads the ClaimTypes.Role claims embedded in the JWT at
            // login (see JwtTokenGenerator), so this check costs no database call.
            // Every other endpoint uses plain .RequireAuthorization() — any signed-in
            // user, no specific role.
            .RequireAuthorization(policy => policy.RequireRole(Roles.Admin))
            .Produces<CreateProductResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem();
    }
}
