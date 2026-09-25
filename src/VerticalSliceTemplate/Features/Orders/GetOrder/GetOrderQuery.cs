using MediatR;

namespace VerticalSliceTemplate.Features.Orders.GetOrder;

public record GetOrderQuery(int Id) : IRequest<GetOrderResponse?>;
