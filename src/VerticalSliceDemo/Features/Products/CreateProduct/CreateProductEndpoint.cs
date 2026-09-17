using MediatR;
using VerticalSliceDemo.Data.Entities;
using VerticalSliceDemo.Features.Shared.Abstractions;

namespace VerticalSliceDemo.Features.Products.CreateProduct;

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
            .RequireAuthorization(policy => policy.RequireRole(Roles.Admin))
            .Produces<CreateProductResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem();
    }
}
