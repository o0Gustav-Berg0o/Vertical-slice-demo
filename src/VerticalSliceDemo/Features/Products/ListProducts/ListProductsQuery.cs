using MediatR;

namespace VerticalSliceDemo.Features.Products.ListProducts;

public record ListProductsQuery : IRequest<IReadOnlyList<ListProductsResponse>>;
