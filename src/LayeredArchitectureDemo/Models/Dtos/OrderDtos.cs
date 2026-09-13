namespace LayeredArchitectureDemo.Models.Dtos;

public record CreateOrderRequest(string CustomerName, List<CreateOrderLineItemRequest> Items);

public record CreateOrderLineItemRequest(int ProductId, int Quantity);

public record OrderResponse(
    int Id,
    string CustomerName,
    decimal TotalAmount,
    DateTime CreatedAt,
    List<OrderItemResponse> Items);

public record OrderItemResponse(int ProductId, string ProductName, int Quantity, decimal UnitPrice);
