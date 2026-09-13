namespace VerticalSliceDemo.Features.Products.CreateProduct;

public record CreateProductResponse(
    int Id,
    string Name,
    string Description,
    decimal Price,
    int StockQuantity,
    DateTime CreatedAt);
