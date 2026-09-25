namespace VerticalSliceTemplate.Features.Products.CreateProduct;

// This folder (CreateProduct/) is the reference example for the whole app —
// copy it as a starting point for your own feature. Every write-operation
// slice has this same shape: a Request (this file) carrying the input, plus
// a Validator, a Response, and an Endpoint that does the actual work (a query
// slice like GetProduct/ follows the same shape, just with nothing to
// validate in some cases).
//
// The Request is a plain record — no interface to implement. FastEndpoints
// binds the incoming JSON body (or route/query values) to this type based on
// the generic parameters of the Endpoint<TRequest, TResponse> class below.
public record CreateProductRequest(
    string Name,
    string Description,
    decimal Price,
    int StockQuantity);
