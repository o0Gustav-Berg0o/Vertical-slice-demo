using MediatR;
using Microsoft.EntityFrameworkCore;
using VerticalSliceDemo.Data;

namespace VerticalSliceDemo.Features.Products.GetProduct;

public class GetProductHandler(AppDbContext db) : IRequestHandler<GetProductQuery, GetProductResponse?>
{
    public async Task<GetProductResponse?> Handle(GetProductQuery request, CancellationToken cancellationToken)
    {
        return await db.Products
            .Where(p => p.Id == request.Id)
            .Select(p => new GetProductResponse(
                p.Id,
                p.Name,
                p.Description,
                p.Price,
                p.StockQuantity,
                p.CreatedAt))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
