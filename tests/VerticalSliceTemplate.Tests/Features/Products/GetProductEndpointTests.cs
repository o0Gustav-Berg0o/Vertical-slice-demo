using FastEndpoints;
using VerticalSliceTemplate.Data.Entities;
using VerticalSliceTemplate.Features.Products.GetProduct;
using VerticalSliceTemplate.Tests.Common;
using Xunit;

namespace VerticalSliceTemplate.Tests.Features.Products;

public class GetProductEndpointTests
{
    [Fact]
    public async Task HandleAsync_ExistingProduct_ReturnsResponse()
    {
        using var db = TestDbContextFactory.Create();
        var product = new Product { Name = "Widget", Description = "desc", Price = 5m, StockQuantity = 3 };
        db.Products.Add(product);
        await db.SaveChangesAsync();

        var ep = Factory.Create<GetProductEndpoint>(db);
        await ep.HandleAsync(new GetProductRequest(product.Id), CancellationToken.None);

        Assert.Equal("Widget", ep.Response.Name);
    }

    [Fact]
    public async Task HandleAsync_MissingProduct_Returns404()
    {
        using var db = TestDbContextFactory.Create();
        var ep = Factory.Create<GetProductEndpoint>(db);

        await ep.HandleAsync(new GetProductRequest(999), CancellationToken.None);

        Assert.Equal(404, ep.HttpContext.Response.StatusCode);
    }
}
