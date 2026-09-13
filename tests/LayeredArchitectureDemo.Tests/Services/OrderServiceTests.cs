using LayeredArchitectureDemo.Common;
using LayeredArchitectureDemo.Models.Dtos;
using LayeredArchitectureDemo.Models.Entities;
using LayeredArchitectureDemo.Repositories;
using LayeredArchitectureDemo.Services;
using LayeredArchitectureDemo.Tests.Common;
using LayeredArchitectureDemo.Validators;
using Xunit;

namespace LayeredArchitectureDemo.Tests.Services;

public class OrderServiceTests
{
    private static OrderService CreateService(LayeredArchitectureDemo.Data.AppDbContext db) =>
        new(new OrderRepository(db), new ProductRepository(db), new CreateOrderRequestValidator());

    [Fact]
    public async Task CreateAsync_ValidOrder_ComputesTotalAndPersists()
    {
        using var db = TestDbContextFactory.Create();
        var product = new Product { Name = "Widget", Price = 10m, StockQuantity = 50 };
        db.Products.Add(product);
        await db.SaveChangesAsync();

        var service = CreateService(db);
        var request = new CreateOrderRequest("Ada Lovelace", [new CreateOrderLineItemRequest(product.Id, 3)]);

        var result = await service.CreateAsync(request, CancellationToken.None);

        Assert.Equal(30m, result.TotalAmount);
        Assert.Single(result.Items);
        Assert.Equal("Widget", result.Items[0].ProductName);
        Assert.Single(db.Orders);
    }

    [Fact]
    public async Task CreateAsync_UnknownProduct_ThrowsNotFoundException()
    {
        using var db = TestDbContextFactory.Create();
        var service = CreateService(db);
        var request = new CreateOrderRequest("Ada Lovelace", [new CreateOrderLineItemRequest(999, 1)]);

        await Assert.ThrowsAsync<NotFoundException>(() => service.CreateAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task CreateAsync_NoItems_ThrowsValidationException()
    {
        using var db = TestDbContextFactory.Create();
        var service = CreateService(db);
        var request = new CreateOrderRequest("Ada Lovelace", []);

        await Assert.ThrowsAsync<FluentValidation.ValidationException>(
            () => service.CreateAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task GetByIdAsync_MissingOrder_ReturnsNull()
    {
        using var db = TestDbContextFactory.Create();
        var service = CreateService(db);

        var result = await service.GetByIdAsync(999, CancellationToken.None);

        Assert.Null(result);
    }
}
