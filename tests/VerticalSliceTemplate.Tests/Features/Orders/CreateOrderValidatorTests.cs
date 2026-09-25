using VerticalSliceTemplate.Features.Orders.CreateOrder;
using Xunit;

namespace VerticalSliceTemplate.Tests.Features.Orders;

public class CreateOrderValidatorTests
{
    private readonly CreateOrderValidator _validator = new();

    [Fact]
    public void Validate_ValidRequest_HasNoErrors()
    {
        var request = new CreateOrderRequest("Ada Lovelace", [new CreateOrderLineItem(1, 2)]);

        var result = _validator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_NoItems_HasError()
    {
        var request = new CreateOrderRequest("Ada Lovelace", []);

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_MissingCustomerName_HasError()
    {
        var request = new CreateOrderRequest("", [new CreateOrderLineItem(1, 2)]);

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
    }
}
