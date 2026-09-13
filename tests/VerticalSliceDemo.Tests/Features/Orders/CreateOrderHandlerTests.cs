using VerticalSliceDemo.Data.Entities;
using VerticalSliceDemo.Features.Orders.CreateOrder;
using VerticalSliceDemo.Features.Shared.Abstractions;
using VerticalSliceDemo.Tests.Common;
using Xunit;

namespace VerticalSliceDemo.Tests.Features.Orders;

public class CreateOrderHandlerTests
{
    [Fact]
    public async Task Handle_ValidOrder_ComputesTotalAndPersists()
    {
        using var db = TestDbContextFactory.Create();
        var product = new Product { Name = "Widget", Price = 10m, StockQuantity = 50 };
        db.Products.Add(product);
        await db.SaveChangesAsync();

        var handler = new CreateOrderHandler(db);
        var command = new CreateOrderCommand("Ada Lovelace", [new CreateOrderLineItem(product.Id, 3)]);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.Equal(30m, result.TotalAmount);
        Assert.Single(result.Items);
        Assert.Equal("Widget", result.Items[0].ProductName);
        Assert.Single(db.Orders);
    }

    [Fact]
    public async Task Handle_UnknownProduct_ThrowsNotFoundException()
    {
        using var db = TestDbContextFactory.Create();
        var handler = new CreateOrderHandler(db);
        var command = new CreateOrderCommand("Ada Lovelace", [new CreateOrderLineItem(999, 1)]);

        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None));
    }
}
