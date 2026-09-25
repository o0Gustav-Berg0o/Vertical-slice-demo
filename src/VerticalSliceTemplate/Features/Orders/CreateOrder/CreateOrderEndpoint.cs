using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using VerticalSliceTemplate.Data;
using VerticalSliceTemplate.Data.Entities;
using VerticalSliceTemplate.Features.Shared.Abstractions;

namespace VerticalSliceTemplate.Features.Orders.CreateOrder;

public class CreateOrderEndpoint(AppDbContext db) : Endpoint<CreateOrderRequest, CreateOrderResponse>
{
    public override void Configure()
    {
        Post("/api/orders");
    }

    public override async Task HandleAsync(CreateOrderRequest req, CancellationToken ct)
    {
        var productIds = req.Items.Select(i => i.ProductId).ToList();
        var products = await db.Products
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, ct);

        var missingIds = productIds.Except(products.Keys).ToList();
        if (missingIds.Count > 0)
        {
            throw new NotFoundException($"Product(s) not found: {string.Join(", ", missingIds)}");
        }

        var order = new VerticalSliceTemplate.Data.Entities.Order
        {
            CustomerName = req.CustomerName,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var item in req.Items)
        {
            var product = products[item.ProductId];
            order.Items.Add(new OrderItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Quantity = item.Quantity,
                UnitPrice = product.Price
            });
        }

        order.TotalAmount = order.Items.Sum(i => i.Quantity * i.UnitPrice);

        db.Orders.Add(order);
        await db.SaveChangesAsync(ct);

        var response = new CreateOrderResponse(
            order.Id,
            order.CustomerName,
            order.TotalAmount,
            order.CreatedAt,
            order.Items.Select(i => new CreateOrderItemResponse(i.ProductId, i.ProductName, i.Quantity, i.UnitPrice)).ToList());

        await Send.ResponseAsync(response, StatusCodes.Status201Created, ct);
    }
}
