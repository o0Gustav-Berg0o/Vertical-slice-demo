using MediatR;

namespace VerticalSliceDemo.Features.Products.GetProduct;

public record GetProductQuery(int Id) : IRequest<GetProductResponse?>;
