# Vertical Slice Demo

A .NET 10 Web API demonstrating **Vertical Slice Architecture (VSA)**: each feature owns its
request/response, handler, and validation in one folder, instead of being spread across
layered Controllers/Services/Repositories.

## Structure

```
src/VerticalSliceDemo/
├── Features/
│   ├── Products/
│   │   ├── CreateProduct/   (Command, Handler, Validator, Response, Endpoint)
│   │   ├── GetProduct/      (Query, Handler, Response, Endpoint)
│   │   └── ListProducts/    (Query, Handler, Response, Endpoint)
│   ├── Orders/
│   │   ├── CreateOrder/     (Command, Handler, Validator, Response, Endpoint)
│   │   └── GetOrder/        (Query, Handler, Response, Endpoint)
│   └── Shared/
│       ├── Abstractions/    (IEndpoint, ValidationBehavior, NotFoundException)
│       └── Extensions/      (DI + endpoint-mapping helpers)
├── Data/
│   ├── AppDbContext.cs
│   ├── AppDbContextFactory.cs   (EF Core design-time factory)
│   ├── Entities/
│   └── Migrations/
├── Program.cs
└── appsettings.json

tests/VerticalSliceDemo.Tests/
├── Features/Products/   (handler + validator tests)
├── Features/Orders/     (handler + validator tests)
└── Common/              (in-memory DbContext test helper)
```

## Stack

- **.NET 10** minimal APIs
- **MediatR** for in-process command/query dispatch
- **FluentValidation**, wired in as a MediatR pipeline behavior so every command/query is
  validated before it reaches its handler
- **EF Core (SQLite)** for persistence, **EF Core InMemory** for tests
- **xUnit** for unit tests

## Running

```bash
cd src/VerticalSliceDemo
dotnet run
```

The database is created automatically on startup in the Development environment. The OpenAPI
document is served at `/openapi/v1.json` in Development.

### Example requests

```bash
curl -X POST http://localhost:5000/api/products \
  -H "Content-Type: application/json" \
  -d '{"name":"Widget","description":"A useful widget","price":9.99,"stockQuantity":100}'

curl http://localhost:5000/api/products

curl -X POST http://localhost:5000/api/orders \
  -H "Content-Type: application/json" \
  -d '{"customerName":"Ada Lovelace","items":[{"productId":1,"quantity":2}]}'
```

## Testing

```bash
dotnet test
```

## Adding a new feature slice

1. Create a folder under `Features/<Area>/<FeatureName>/`.
2. Add a `Command`/`Query` (implements `IRequest<TResponse>`), a `Response` record, a
   `Handler` (implements `IRequestHandler<,>`), and optionally a `Validator`
   (`AbstractValidator<T>`).
3. Add an `Endpoint` class implementing `IEndpoint` and map the route in `MapEndpoint`.
4. It's picked up automatically — `AddFeatures` and `MapFeatureEndpoints` scan the assembly
   for handlers, validators, and endpoints, so nothing else needs to be registered.
