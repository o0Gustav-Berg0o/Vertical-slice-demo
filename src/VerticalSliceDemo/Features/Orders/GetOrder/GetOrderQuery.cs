using MediatR;

namespace VerticalSliceDemo.Features.Orders.GetOrder;

public record GetOrderQuery(int Id) : IRequest<GetOrderResponse?>;
