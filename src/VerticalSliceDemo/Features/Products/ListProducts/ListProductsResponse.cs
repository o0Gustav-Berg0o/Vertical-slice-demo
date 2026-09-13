namespace VerticalSliceDemo.Features.Products.ListProducts;

public record ListProductsResponse(
    int Id,
    string Name,
    decimal Price,
    int StockQuantity);
