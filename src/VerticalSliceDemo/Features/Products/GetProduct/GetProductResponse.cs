namespace VerticalSliceDemo.Features.Products.GetProduct;

public record GetProductResponse(
    int Id,
    string Name,
    string Description,
    decimal Price,
    int StockQuantity,
    DateTime CreatedAt);
