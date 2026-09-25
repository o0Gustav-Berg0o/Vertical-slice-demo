using MediatR;

namespace VerticalSliceTemplate.Features.Products.ListProducts;

public record ListProductsQuery : IRequest<IReadOnlyList<ListProductsResponse>>;
