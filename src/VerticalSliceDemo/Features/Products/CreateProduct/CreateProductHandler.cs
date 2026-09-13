using MediatR;
using VerticalSliceDemo.Data;
using VerticalSliceDemo.Data.Entities;

namespace VerticalSliceDemo.Features.Products.CreateProduct;

public class CreateProductHandler(AppDbContext db) : IRequestHandler<CreateProductCommand, CreateProductResponse>
{
    public async Task<CreateProductResponse> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = new Product
        {
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            StockQuantity = request.StockQuantity,
            CreatedAt = DateTime.UtcNow
        };

        db.Products.Add(product);
        await db.SaveChangesAsync(cancellationToken);

        return new CreateProductResponse(
            product.Id,
            product.Name,
            product.Description,
            product.Price,
            product.StockQuantity,
            product.CreatedAt);
    }
}
