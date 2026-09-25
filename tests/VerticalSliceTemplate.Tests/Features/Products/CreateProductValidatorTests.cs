using VerticalSliceTemplate.Features.Products.CreateProduct;
using Xunit;

namespace VerticalSliceTemplate.Tests.Features.Products;

public class CreateProductValidatorTests
{
    private readonly CreateProductValidator _validator = new();

    [Fact]
    public void Validate_ValidRequest_HasNoErrors()
    {
        var request = new CreateProductRequest("Widget", "A useful widget", 9.99m, 10);

        var result = _validator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("", 9.99, 10)]
    [InlineData("Widget", 0, 10)]
    [InlineData("Widget", -5, 10)]
    [InlineData("Widget", 9.99, -1)]
    public void Validate_InvalidRequest_HasErrors(string name, decimal price, int stock)
    {
        var request = new CreateProductRequest(name, "desc", price, stock);

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
    }
}
