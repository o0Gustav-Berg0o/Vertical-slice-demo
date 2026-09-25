using FastEndpoints;
using VerticalSliceTemplate.Data;
using VerticalSliceTemplate.Data.Entities;

namespace VerticalSliceTemplate.Features.Products.CreateProduct;

// FastEndpoints finds this class by scanning the assembly for types deriving
// from Endpoint<TRequest, TResponse> — nothing to register in Program.cs.
// Configure() runs once at startup (route + auth rules); HandleAsync runs on
// every request. There's no separate Handler class: the business logic lives
// directly in HandleAsync, since FastEndpoints already gives each endpoint
// its own single-responsibility class — a mediator would just be a second,
// redundant layer of indirection on top of that.
public class CreateProductEndpoint(AppDbContext db) : Endpoint<CreateProductRequest, CreateProductResponse>
{
    public override void Configure()
    {
        Post("/api/products");

        // The one endpoint in this template that needs more than "logged in":
        // Roles reads the ClaimTypes.Role claims embedded in the JWT at login
        // (see JwtTokenGenerator), so this check costs no database call. Every
        // other endpoint is secure-by-default with no extra role requirement —
        // any signed-in user. FastEndpoints requires authentication on every
        // endpoint unless AllowAnonymous() is called (see LoginEndpoint).
        Roles(VerticalSliceTemplate.Data.Entities.Roles.Admin);
    }

    public override async Task HandleAsync(CreateProductRequest req, CancellationToken ct)
    {
        var product = new Product
        {
            Name = req.Name,
            Description = req.Description,
            Price = req.Price,
            StockQuantity = req.StockQuantity,
            CreatedAt = DateTime.UtcNow
        };

        db.Products.Add(product);
        await db.SaveChangesAsync(ct);

        var response = new CreateProductResponse(
            product.Id,
            product.Name,
            product.Description,
            product.Price,
            product.StockQuantity,
            product.CreatedAt);

        // 201 Created with the new product in the body. (Skipping the Location
        // header here for simplicity — Send.CreatedAtAsync can add one if you
        // want to point it at GetProductEndpoint.)
        await Send.ResponseAsync(response, StatusCodes.Status201Created, ct);
    }
}
