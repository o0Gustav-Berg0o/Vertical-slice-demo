using MediatR;
using VerticalSliceDemo.Features.Shared.Abstractions;

namespace VerticalSliceDemo.Features.Products.ListProducts;

public class ListProductsEndpoint : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/products", async (ISender sender) =>
            {
                var result = await sender.Send(new ListProductsQuery());
                return Results.Ok(result);
            })
            .WithName("ListProducts")
            .WithTags("Products")
            .RequireAuthorization()
            .Produces<IReadOnlyList<ListProductsResponse>>();
    }
}
