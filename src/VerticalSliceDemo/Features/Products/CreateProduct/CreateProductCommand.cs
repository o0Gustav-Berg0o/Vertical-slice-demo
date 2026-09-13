using MediatR;

namespace VerticalSliceDemo.Features.Products.CreateProduct;

public record CreateProductCommand(
    string Name,
    string Description,
    decimal Price,
    int StockQuantity) : IRequest<CreateProductResponse>;
