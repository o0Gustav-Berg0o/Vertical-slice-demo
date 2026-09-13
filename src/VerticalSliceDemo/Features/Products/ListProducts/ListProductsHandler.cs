using MediatR;
using Microsoft.EntityFrameworkCore;
using VerticalSliceDemo.Data;

namespace VerticalSliceDemo.Features.Products.ListProducts;

public class ListProductsHandler(AppDbContext db)
    : IRequestHandler<ListProductsQuery, IReadOnlyList<ListProductsResponse>>
{
    public async Task<IReadOnlyList<ListProductsResponse>> Handle(
        ListProductsQuery request,
        CancellationToken cancellationToken)
    {
        return await db.Products
            .OrderBy(p => p.Name)
            .Select(p => new ListProductsResponse(p.Id, p.Name, p.Price, p.StockQuantity))
            .ToListAsync(cancellationToken);
    }
}
