using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using VerticalSliceTemplate.Data;

namespace VerticalSliceTemplate.Features.Products.ListProducts;

// EndpointWithoutRequest<TResponse> instead of Endpoint<TRequest, TResponse> —
// this operation takes no input at all, so there's no Request type/file for it.
public class ListProductsEndpoint(AppDbContext db) : EndpointWithoutRequest<List<ListProductsResponse>>
{
    public override void Configure()
    {
        Get("/api/products");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var products = await db.Products
            .OrderBy(p => p.Name)
            .Select(p => new ListProductsResponse(p.Id, p.Name, p.Price, p.StockQuantity))
            .ToListAsync(ct);

        await Send.OkAsync(products, ct);
    }
}
