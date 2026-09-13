using MediatR;
using Microsoft.EntityFrameworkCore;
using VerticalSliceDemo.Data;
using VerticalSliceDemo.Data.Entities;
using VerticalSliceDemo.Features.Shared.Abstractions;

namespace VerticalSliceDemo.Features.Orders.CreateOrder;

public class CreateOrderHandler(AppDbContext db) : IRequestHandler<CreateOrderCommand, CreateOrderResponse>
{
    public async Task<CreateOrderResponse> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var productIds = request.Items.Select(i => i.ProductId).ToList();
        var products = await db.Products
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, cancellationToken);

        var missingIds = productIds.Except(products.Keys).ToList();
        if (missingIds.Count > 0)
        {
            throw new NotFoundException($"Product(s) not found: {string.Join(", ", missingIds)}");
        }

        var order = new Order
        {
            CustomerName = request.CustomerName,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var item in request.Items)
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
        await db.SaveChangesAsync(cancellationToken);

        return new CreateOrderResponse(
            order.Id,
            order.CustomerName,
            order.TotalAmount,
            order.CreatedAt,
            order.Items.Select(i => new CreateOrderItemResponse(i.ProductId, i.ProductName, i.Quantity, i.UnitPrice)).ToList());
    }
}
