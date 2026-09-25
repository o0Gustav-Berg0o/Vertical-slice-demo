using FastEndpoints;
using VerticalSliceTemplate.Data.Entities;
using VerticalSliceTemplate.Features.Orders.GetOrder;
using VerticalSliceTemplate.Tests.Common;
using Xunit;

namespace VerticalSliceTemplate.Tests.Features.Orders;

public class GetOrderEndpointTests
{
    [Fact]
    public async Task HandleAsync_ExistingOrder_ReturnsResponseWithItems()
    {
        using var db = TestDbContextFactory.Create();
        var order = new VerticalSliceTemplate.Data.Entities.Order
        {
            CustomerName = "Ada Lovelace",
            TotalAmount = 20m,
            Items = [new OrderItem { ProductId = 1, ProductName = "Widget", Quantity = 2, UnitPrice = 10m }]
        };
        db.Orders.Add(order);
        await db.SaveChangesAsync();

        var ep = Factory.Create<GetOrderEndpoint>(db);
        await ep.HandleAsync(new GetOrderRequest(order.Id), CancellationToken.None);

        Assert.Equal("Ada Lovelace", ep.Response.CustomerName);
        Assert.Single(ep.Response.Items);
    }

    [Fact]
    public async Task HandleAsync_MissingOrder_Returns404()
    {
        using var db = TestDbContextFactory.Create();
        var ep = Factory.Create<GetOrderEndpoint>(db);

        await ep.HandleAsync(new GetOrderRequest(999), CancellationToken.None);

        Assert.Equal(404, ep.HttpContext.Response.StatusCode);
    }
}
