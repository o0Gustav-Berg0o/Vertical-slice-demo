using VerticalSliceDemo.Features.Products.CreateProduct;
using VerticalSliceDemo.Tests.Common;
using Xunit;

namespace VerticalSliceDemo.Tests.Features.Products;

public class CreateProductHandlerTests
{
    [Fact]
    public async Task Handle_ValidCommand_PersistsAndReturnsProduct()
    {
        using var db = TestDbContextFactory.Create();
        var handler = new CreateProductHandler(db);
        var command = new CreateProductCommand("Widget", "A useful widget", 9.99m, 100);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.NotEqual(0, result.Id);
        Assert.Equal("Widget", result.Name);
        Assert.Equal(9.99m, result.Price);
        Assert.Single(db.Products);
    }
}
