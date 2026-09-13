using VerticalSliceDemo.Features.Orders.CreateOrder;
using Xunit;

namespace VerticalSliceDemo.Tests.Features.Orders;

public class CreateOrderValidatorTests
{
    private readonly CreateOrderValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_HasNoErrors()
    {
        var command = new CreateOrderCommand("Ada Lovelace", [new CreateOrderLineItem(1, 2)]);

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_NoItems_HasError()
    {
        var command = new CreateOrderCommand("Ada Lovelace", []);

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_MissingCustomerName_HasError()
    {
        var command = new CreateOrderCommand("", [new CreateOrderLineItem(1, 2)]);

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
    }
}
