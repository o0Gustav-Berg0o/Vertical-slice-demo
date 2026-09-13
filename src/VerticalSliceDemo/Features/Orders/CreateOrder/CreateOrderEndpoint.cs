using MediatR;
using VerticalSliceDemo.Features.Shared.Abstractions;

namespace VerticalSliceDemo.Features.Orders.CreateOrder;

public class CreateOrderEndpoint : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/orders", async (CreateOrderCommand command, ISender sender) =>
            {
                var result = await sender.Send(command);
                return Results.Created($"/api/orders/{result.Id}", result);
            })
            .WithName("CreateOrder")
            .WithTags("Orders")
            .RequireAuthorization()
            .Produces<CreateOrderResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status404NotFound);
    }
}
