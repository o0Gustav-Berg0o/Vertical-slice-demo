using MediatR;
using Microsoft.EntityFrameworkCore;
using VerticalSliceDemo.Data;

namespace VerticalSliceDemo.Features.Orders.GetOrder;

public class GetOrderHandler(AppDbContext db) : IRequestHandler<GetOrderQuery, GetOrderResponse?>
{
    public async Task<GetOrderResponse?> Handle(GetOrderQuery request, CancellationToken cancellationToken)
    {
        var order = await db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

        if (order is null)
        {
            return null;
        }

        return new GetOrderResponse(
            order.Id,
            order.CustomerName,
            order.TotalAmount,
            order.CreatedAt,
            order.Items
                .Select(i => new GetOrderItemResponse(i.ProductId, i.ProductName, i.Quantity, i.UnitPrice))
                .ToList());
    }
}
