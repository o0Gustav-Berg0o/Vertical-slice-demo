using FluentValidation;
using LayeredArchitectureDemo.Models.Dtos;
using LayeredArchitectureDemo.Models.Entities;
using LayeredArchitectureDemo.Repositories;

namespace LayeredArchitectureDemo.Services;

public class ProductService(IProductRepository repository, IValidator<CreateProductRequest> validator)
    : IProductService
{
    public async Task<ProductResponse?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var product = await repository.GetByIdAsync(id, cancellationToken);
        return product is null ? null : ToResponse(product);
    }

    public async Task<List<ProductSummaryResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        var products = await repository.GetAllAsync(cancellationToken);
        return products
            .Select(p => new ProductSummaryResponse(p.Id, p.Name, p.Price, p.StockQuantity))
            .ToList();
    }

    public async Task<ProductResponse> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var product = new Product
        {
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            StockQuantity = request.StockQuantity,
            CreatedAt = DateTime.UtcNow
        };

        await repository.AddAsync(product, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return ToResponse(product);
    }

    private static ProductResponse ToResponse(Product product) => new(
        product.Id,
        product.Name,
        product.Description,
        product.Price,
        product.StockQuantity,
        product.CreatedAt);
}
