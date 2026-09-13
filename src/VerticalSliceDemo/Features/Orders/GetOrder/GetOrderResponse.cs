namespace VerticalSliceDemo.Features.Orders.GetOrder;

public record GetOrderResponse(
    int Id,
    string CustomerName,
    decimal TotalAmount,
    DateTime CreatedAt,
    List<GetOrderItemResponse> Items);

public record GetOrderItemResponse(int ProductId, string ProductName, int Quantity, decimal UnitPrice);
