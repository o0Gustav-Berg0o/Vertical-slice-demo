using MediatR;
using VerticalSliceDemo.Features.Shared.Abstractions;

namespace VerticalSliceDemo.Features.Orders.GetOrder;

public class GetOrderEndpoint : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/orders/{id:int}", async (int id, ISender sender) =>
            {
                var result = await sender.Send(new GetOrderQuery(id));
                return result is not null ? Results.Ok(result) : Results.NotFound();
            })
            .WithName("GetOrder")
            .WithTags("Orders")
            .RequireAuthorization()
            .Produces<GetOrderResponse>()
            .Produces(StatusCodes.Status404NotFound);
    }
}
