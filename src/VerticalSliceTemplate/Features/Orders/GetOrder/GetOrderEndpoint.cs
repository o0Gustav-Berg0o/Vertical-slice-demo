using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using VerticalSliceTemplate.Data;

namespace VerticalSliceTemplate.Features.Orders.GetOrder;

public class GetOrderEndpoint(AppDbContext db) : Endpoint<GetOrderRequest, GetOrderResponse>
{
    public override void Configure()
    {
        Get("/api/orders/{id}");
    }

    public override async Task HandleAsync(GetOrderRequest req, CancellationToken ct)
    {
        var order = await db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == req.Id, ct);

        if (order is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var response = new GetOrderResponse(
            order.Id,
            order.CustomerName,
            order.TotalAmount,
            order.CreatedAt,
            order.Items
                .Select(i => new GetOrderItemResponse(i.ProductId, i.ProductName, i.Quantity, i.UnitPrice))
                .ToList());

        await Send.OkAsync(response, ct);
    }
}
