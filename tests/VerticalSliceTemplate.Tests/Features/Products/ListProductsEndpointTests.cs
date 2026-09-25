using FastEndpoints;
using VerticalSliceTemplate.Data.Entities;
using VerticalSliceTemplate.Features.Products.ListProducts;
using VerticalSliceTemplate.Tests.Common;
using Xunit;

namespace VerticalSliceTemplate.Tests.Features.Products;

public class ListProductsEndpointTests
{
    [Fact]
    public async Task HandleAsync_ReturnsProductsOrderedByName()
    {
        using var db = TestDbContextFactory.Create();
        db.Products.AddRange(
            new Product { Name = "Zebra", Price = 1m, StockQuantity = 1 },
            new Product { Name = "Apple", Price = 2m, StockQuantity = 2 });
        await db.SaveChangesAsync();

        var ep = Factory.Create<ListProductsEndpoint>(db);
        await ep.HandleAsync(CancellationToken.None);

        Assert.Equal(2, ep.Response.Count);
        Assert.Equal("Apple", ep.Response[0].Name);
        Assert.Equal("Zebra", ep.Response[1].Name);
    }
}
