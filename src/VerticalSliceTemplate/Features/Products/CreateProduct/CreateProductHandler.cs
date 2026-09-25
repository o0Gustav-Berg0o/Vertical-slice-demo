using MediatR;
using VerticalSliceTemplate.Data;
using VerticalSliceTemplate.Data.Entities;

namespace VerticalSliceTemplate.Features.Products.CreateProduct;

// The actual business logic for this one operation, and nothing else — no
// shared "ProductService"/"ProductRepository" layer to route through. It
// talks to AppDbContext directly. Because each slice is small and
// self-contained, a handler like this rarely grows beyond what fits on one
// screen; if it does, that's a sign the operation itself is doing too much,
// not a sign you need a service layer.
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
