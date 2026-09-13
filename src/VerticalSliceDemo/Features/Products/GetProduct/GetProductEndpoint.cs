using MediatR;
using VerticalSliceDemo.Features.Shared.Abstractions;

namespace VerticalSliceDemo.Features.Products.GetProduct;

public class GetProductEndpoint : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/products/{id:int}", async (int id, ISender sender) =>
            {
                var result = await sender.Send(new GetProductQuery(id));
                return result is not null ? Results.Ok(result) : Results.NotFound();
            })
            .WithName("GetProduct")
            .WithTags("Products")
            .Produces<GetProductResponse>()
            .Produces(StatusCodes.Status404NotFound);
    }
}
