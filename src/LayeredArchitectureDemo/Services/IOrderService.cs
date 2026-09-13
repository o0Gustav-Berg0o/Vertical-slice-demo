using LayeredArchitectureDemo.Models.Dtos;

namespace LayeredArchitectureDemo.Services;

public interface IOrderService
{
    Task<OrderResponse?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<OrderResponse> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken);
}
