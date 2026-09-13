using VerticalSliceDemo.Data.Entities;
using VerticalSliceDemo.Features.Orders.GetOrder;
using VerticalSliceDemo.Tests.Common;
using Xunit;

namespace VerticalSliceDemo.Tests.Features.Orders;

public class GetOrderHandlerTests
{
    [Fact]
    public async Task Handle_ExistingOrder_ReturnsResponseWithItems()
    {
        using var db = TestDbContextFactory.Create();
        var order = new Order
        {
            CustomerName = "Ada Lovelace",
            TotalAmount = 20m,
            Items = [new OrderItem { ProductId = 1, ProductName = "Widget", Quantity = 2, UnitPrice = 10m }]
        };
        db.Orders.Add(order);
        await db.SaveChangesAsync();

        var handler = new GetOrderHandler(db);
        var result = await handler.Handle(new GetOrderQuery(order.Id), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("Ada Lovelace", result!.CustomerName);
        Assert.Single(result.Items);
    }

    [Fact]
    public async Task Handle_MissingOrder_ReturnsNull()
    {
        using var db = TestDbContextFactory.Create();
        var handler = new GetOrderHandler(db);

        var result = await handler.Handle(new GetOrderQuery(999), CancellationToken.None);

        Assert.Null(result);
    }
}
