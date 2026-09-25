using MediatR;

namespace VerticalSliceTemplate.Features.Products.CreateProduct;

// This folder (CreateProduct/) is the reference example for the whole app —
// copy it as a starting point for your own feature. Every write-operation
// slice has this same shape: a Command (this file) carrying the input, plus
// a Validator, a Handler, a Response, and an Endpoint (a query slice like
// GetProduct/ uses a Query instead of a Command, but the other four files
// are the same idea).
//
// The Command is just the input to the operation as an IRequest<TResponse> —
// MediatR uses the generic parameter to know which Handler's Handle() method
// to call and what type it returns.
public record CreateProductCommand(
    string Name,
    string Description,
    decimal Price,
    int StockQuantity) : IRequest<CreateProductResponse>;
