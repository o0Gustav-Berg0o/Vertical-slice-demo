using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using VerticalSliceTemplate.Data;

namespace VerticalSliceTemplate.Features.Products.GetProduct;

public class GetProductEndpoint(AppDbContext db) : Endpoint<GetProductRequest, GetProductResponse>
{
    public override void Configure()
    {
        Get("/api/products/{id}");
        // No Roles(...) call — just secure-by-default, any signed-in user.
    }

    public override async Task HandleAsync(GetProductRequest req, CancellationToken ct)
    {
        var product = await db.Products
            .Where(p => p.Id == req.Id)
            .Select(p => new GetProductResponse(p.Id, p.Name, p.Description, p.Price, p.StockQuantity, p.CreatedAt))
            .FirstOrDefaultAsync(ct);

        if (product is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(product, ct);
    }
}
