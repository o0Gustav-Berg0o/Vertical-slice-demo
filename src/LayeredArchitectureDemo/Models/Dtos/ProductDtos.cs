namespace LayeredArchitectureDemo.Models.Dtos;

public record CreateProductRequest(string Name, string Description, decimal Price, int StockQuantity);

public record ProductResponse(
    int Id,
    string Name,
    string Description,
    decimal Price,
    int StockQuantity,
    DateTime CreatedAt);

public record ProductSummaryResponse(int Id, string Name, decimal Price, int StockQuantity);
