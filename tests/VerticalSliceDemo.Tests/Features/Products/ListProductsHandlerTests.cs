using VerticalSliceDemo.Data.Entities;
using VerticalSliceDemo.Features.Products.ListProducts;
using VerticalSliceDemo.Tests.Common;
using Xunit;

namespace VerticalSliceDemo.Tests.Features.Products;

public class ListProductsHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsProductsOrderedByName()
    {
        using var db = TestDbContextFactory.Create();
        db.Products.AddRange(
            new Product { Name = "Zebra", Price = 1m, StockQuantity = 1 },
            new Product { Name = "Apple", Price = 2m, StockQuantity = 2 });
        await db.SaveChangesAsync();

        var handler = new ListProductsHandler(db);
        var result = await handler.Handle(new ListProductsQuery(), CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Equal("Apple", result[0].Name);
        Assert.Equal("Zebra", result[1].Name);
    }
}
