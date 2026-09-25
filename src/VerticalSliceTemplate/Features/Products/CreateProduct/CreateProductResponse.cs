namespace VerticalSliceTemplate.Features.Products.CreateProduct;

// The shape returned to the client as JSON — deliberately its own type,
// separate from the Product entity. This lets the API contract evolve
// independently of the database schema (e.g. you can add a computed field
// here, or hide an internal column, without touching Product.cs).
public record CreateProductResponse(
    int Id,
    string Name,
    string Description,
    decimal Price,
    int StockQuantity,
    DateTime CreatedAt);
