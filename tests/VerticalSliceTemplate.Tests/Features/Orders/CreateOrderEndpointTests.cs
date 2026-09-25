using FastEndpoints;
using VerticalSliceTemplate.Data.Entities;
using VerticalSliceTemplate.Features.Orders.CreateOrder;
using VerticalSliceTemplate.Features.Shared.Abstractions;
using VerticalSliceTemplate.Tests.Common;
using Xunit;

namespace VerticalSliceTemplate.Tests.Features.Orders;

public class CreateOrderEndpointTests
{
    [Fact]
    public async Task HandleAsync_ValidOrder_ComputesTotalAndPersists()
    {
        using var db = TestDbContextFactory.Create();
        var product = new Product { Name = "Widget", Price = 10m, StockQuantity = 50 };
        db.Products.Add(product);
        await db.SaveChangesAsync();

        var ep = Factory.Create<CreateOrderEndpoint>(db);
        var req = new CreateOrderRequest("Ada Lovelace", [new CreateOrderLineItem(product.Id, 3)]);

        await ep.HandleAsync(req, CancellationToken.None);

        Assert.Equal(30m, ep.Response.TotalAmount);
        Assert.Single(ep.Response.Items);
        Assert.Equal("Widget", ep.Response.Items[0].ProductName);
        Assert.Single(db.Orders);
    }

    [Fact]
    public async Task HandleAsync_UnknownProduct_ThrowsNotFoundException()
    {
        using var db = TestDbContextFactory.Create();
        var ep = Factory.Create<CreateOrderEndpoint>(db);
        var req = new CreateOrderRequest("Ada Lovelace", [new CreateOrderLineItem(999, 1)]);

        await Assert.ThrowsAsync<NotFoundException>(() => ep.HandleAsync(req, CancellationToken.None));
    }
}
