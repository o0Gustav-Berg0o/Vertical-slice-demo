namespace VerticalSliceDemo.Features.Orders.CreateOrder;

public record CreateOrderResponse(
    int Id,
    string CustomerName,
    decimal TotalAmount,
    DateTime CreatedAt,
    List<CreateOrderItemResponse> Items);

public record CreateOrderItemResponse(int ProductId, string ProductName, int Quantity, decimal UnitPrice);
