using LayeredArchitectureDemo.Models.Dtos;
using LayeredArchitectureDemo.Repositories;
using LayeredArchitectureDemo.Services;
using LayeredArchitectureDemo.Tests.Common;
using LayeredArchitectureDemo.Validators;
using Xunit;

namespace LayeredArchitectureDemo.Tests.Services;

public class ProductServiceTests
{
    [Fact]
    public async Task CreateAsync_ValidRequest_PersistsAndReturnsProduct()
    {
        using var db = TestDbContextFactory.Create();
        var service = new ProductService(new ProductRepository(db), new CreateProductRequestValidator());
        var request = new CreateProductRequest("Widget", "A useful widget", 9.99m, 100);

        var result = await service.CreateAsync(request, CancellationToken.None);

        Assert.NotEqual(0, result.Id);
        Assert.Equal("Widget", result.Name);
        Assert.Single(db.Products);
    }

    [Fact]
    public async Task CreateAsync_InvalidRequest_ThrowsValidationException()
    {
        using var db = TestDbContextFactory.Create();
        var service = new ProductService(new ProductRepository(db), new CreateProductRequestValidator());
        var request = new CreateProductRequest("", "bad", -1, -1);

        await Assert.ThrowsAsync<FluentValidation.ValidationException>(
            () => service.CreateAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task GetByIdAsync_MissingProduct_ReturnsNull()
    {
        using var db = TestDbContextFactory.Create();
        var service = new ProductService(new ProductRepository(db), new CreateProductRequestValidator());

        var result = await service.GetByIdAsync(999, CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsProductsOrderedByName()
    {
        using var db = TestDbContextFactory.Create();
        var service = new ProductService(new ProductRepository(db), new CreateProductRequestValidator());
        await service.CreateAsync(new CreateProductRequest("Zebra", "", 1m, 1), CancellationToken.None);
        await service.CreateAsync(new CreateProductRequest("Apple", "", 2m, 2), CancellationToken.None);

        var result = await service.GetAllAsync(CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Equal("Apple", result[0].Name);
        Assert.Equal("Zebra", result[1].Name);
    }
}
