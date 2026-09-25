namespace VerticalSliceTemplate.Features.Products.GetProduct;

// FastEndpoints binds route/query/form values to a request's properties by
// name match (case-insensitive) — Id here comes from the {id} route segment
// declared in GetProductEndpoint.Configure().
public record GetProductRequest(int Id);
