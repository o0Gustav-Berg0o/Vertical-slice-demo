using FastEndpoints;
using VerticalSliceTemplate.Features.Products.CreateProduct;
using VerticalSliceTemplate.Tests.Common;
using Xunit;

namespace VerticalSliceTemplate.Tests.Features.Products;

public class CreateProductEndpointTests
{
    [Fact]
    public async Task HandleAsync_ValidRequest_PersistsAndReturnsProduct()
    {
        using var db = TestDbContextFactory.Create();
        var ep = Factory.Create<CreateProductEndpoint>(db);
        var req = new CreateProductRequest("Widget", "A useful widget", 9.99m, 100);

        await ep.HandleAsync(req, CancellationToken.None);

        Assert.NotEqual(0, ep.Response.Id);
        Assert.Equal("Widget", ep.Response.Name);
        Assert.Equal(9.99m, ep.Response.Price);
        Assert.Single(db.Products);
    }
}
