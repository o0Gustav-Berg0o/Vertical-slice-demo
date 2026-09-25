using VerticalSliceTemplate.Data.Entities;
using VerticalSliceTemplate.Features.Products.GetProduct;
using VerticalSliceTemplate.Tests.Common;
using Xunit;

namespace VerticalSliceTemplate.Tests.Features.Products;

public class GetProductHandlerTests
{
    [Fact]
    public async Task Handle_ExistingProduct_ReturnsResponse()
    {
        using var db = TestDbContextFactory.Create();
        var product = new Product { Name = "Widget", Description = "desc", Price = 5m, StockQuantity = 3 };
        db.Products.Add(product);
        await db.SaveChangesAsync();

        var handler = new GetProductHandler(db);
        var result = await handler.Handle(new GetProductQuery(product.Id), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("Widget", result!.Name);
    }

    [Fact]
    public async Task Handle_MissingProduct_ReturnsNull()
    {
        using var db = TestDbContextFactory.Create();
        var handler = new GetProductHandler(db);

        var result = await handler.Handle(new GetProductQuery(999), CancellationToken.None);

        Assert.Null(result);
    }
}
