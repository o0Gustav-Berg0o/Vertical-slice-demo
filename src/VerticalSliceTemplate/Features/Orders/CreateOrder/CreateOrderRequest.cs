namespace VerticalSliceTemplate.Features.Orders.CreateOrder;

public record CreateOrderRequest(
    string CustomerName,
    List<CreateOrderLineItem> Items);

public record CreateOrderLineItem(int ProductId, int Quantity);
