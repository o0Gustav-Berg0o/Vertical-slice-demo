using VerticalSliceDemo.Features.Products.CreateProduct;
using Xunit;

namespace VerticalSliceDemo.Tests.Features.Products;

public class CreateProductValidatorTests
{
    private readonly CreateProductValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_HasNoErrors()
    {
        var command = new CreateProductCommand("Widget", "A useful widget", 9.99m, 10);

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("", 9.99, 10)]
    [InlineData("Widget", 0, 10)]
    [InlineData("Widget", -5, 10)]
    [InlineData("Widget", 9.99, -1)]
    public void Validate_InvalidCommand_HasErrors(string name, decimal price, int stock)
    {
        var command = new CreateProductCommand(name, "desc", price, stock);

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
    }
}
