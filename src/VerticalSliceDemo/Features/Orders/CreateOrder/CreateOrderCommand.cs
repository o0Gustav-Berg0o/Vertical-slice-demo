using MediatR;

namespace VerticalSliceDemo.Features.Orders.CreateOrder;

public record CreateOrderCommand(
    string CustomerName,
    List<CreateOrderLineItem> Items) : IRequest<CreateOrderResponse>;

public record CreateOrderLineItem(int ProductId, int Quantity);
