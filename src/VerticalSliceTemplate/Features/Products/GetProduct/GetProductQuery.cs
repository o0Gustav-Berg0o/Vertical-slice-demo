using MediatR;

namespace VerticalSliceTemplate.Features.Products.GetProduct;

public record GetProductQuery(int Id) : IRequest<GetProductResponse?>;
