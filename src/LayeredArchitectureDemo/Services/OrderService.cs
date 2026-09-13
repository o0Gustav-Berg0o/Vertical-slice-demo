using FluentValidation;
using LayeredArchitectureDemo.Common;
using LayeredArchitectureDemo.Models.Dtos;
using LayeredArchitectureDemo.Models.Entities;
using LayeredArchitectureDemo.Repositories;

namespace LayeredArchitectureDemo.Services;

public class OrderService(
    IOrderRepository orderRepository,
    IProductRepository productRepository,
    IValidator<CreateOrderRequest> validator) : IOrderService
{
    public async Task<OrderResponse?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(id, cancellationToken);
        return order is null ? null : ToResponse(order);
    }

    public async Task<OrderResponse> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var productIds = request.Items.Select(i => i.ProductId).ToList();
        var products = await productRepository.GetByIdsAsync(productIds, cancellationToken);

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

        await orderRepository.AddAsync(order, cancellationToken);
        await orderRepository.SaveChangesAsync(cancellationToken);

        return ToResponse(order);
    }

    private static OrderResponse ToResponse(Order order) => new(
        order.Id,
        order.CustomerName,
        order.TotalAmount,
        order.CreatedAt,
        order.Items.Select(i => new OrderItemResponse(i.ProductId, i.ProductName, i.Quantity, i.UnitPrice)).ToList());
}
