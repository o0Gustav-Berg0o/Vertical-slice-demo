# Architecture Comparison Demo

Two .NET 10 Web APIs implementing the **identical domain and HTTP contract** (Products,
Orders) with two different architectural styles, plus one frontend that can drive either
one — so you can compare the styles side by side without the API surface changing.

| | Vertical Slice API | Layered API |
|---|---|---|
| Path | `src/VerticalSliceDemo` | `src/LayeredArchitectureDemo` |
| Organized by | Feature | Technical layer |
| Request dispatch | MediatR command/query | ASP.NET controllers |
| Validation | FluentValidation as a MediatR pipeline behavior | FluentValidation invoked explicitly in the service layer |
| Default port | 5010 | 5136 |

Both expose the same routes (`GET/POST /api/products`, `GET /api/products/{id}`,
`POST/GET /api/orders[/{id}]`) with the same JSON shapes, backed by their own SQLite database.

## 1. Vertical Slice API (`src/VerticalSliceDemo`)

Each feature owns its request/response, handler, and validation in one folder, instead of
being spread across layered Controllers/Services/Repositories.

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
```

- **MediatR** dispatches each command/query to its single handler.
- **FluentValidation** validators are picked up by a `ValidationBehavior` in the MediatR
  pipeline, so every request is validated before it reaches its handler.
- Adding a feature: create a folder under `Features/<Area>/<FeatureName>/` with a
  `Command`/`Query`, `Response`, `Handler`, optional `Validator`, and an `Endpoint`
  implementing `IEndpoint`. `AddFeatures`/`MapFeatureEndpoints` scan the assembly and wire
  it up automatically — nothing else needs registering.

```bash
cd src/VerticalSliceDemo
dotnet run   # http://localhost:5010
```

## 2. Layered API (`src/LayeredArchitectureDemo`)

The same domain, organized the traditional way: by technical layer instead of by feature.

```
src/LayeredArchitectureDemo/
├── Controllers/       (ProductsController, OrdersController — HTTP concerns only)
├── Services/          (IProductService/ProductService, IOrderService/OrderService — business logic)
├── Repositories/      (IProductRepository/ProductRepository, IOrderRepository/OrderRepository — data access)
├── Models/
│   ├── Dtos/           (request/response records)
│   └── Entities/       (EF Core entities)
├── Validators/         (FluentValidation validators for request DTOs)
├── Data/
│   ├── AppDbContext.cs
│   ├── AppDbContextFactory.cs
│   └── Migrations/
├── Common/             (NotFoundException)
├── Program.cs
└── appsettings.json
```

- Controllers only translate HTTP <-> DTOs and call a service.
- Services hold business logic: they call `IValidator<T>.ValidateAndThrowAsync` themselves,
  then coordinate one or more repositories.
- Repositories are the only place that talks to `AppDbContext`.
- Adding a feature means touching every layer: a DTO, a validator, a repository method (if
  needed), a service method, and a controller action — the trade-off VSA avoids.

```bash
cd src/LayeredArchitectureDemo
dotnet run   # http://localhost:5136
```

## 3. Frontend (`frontend/`)

A dependency-free static page (plain HTML/CSS/JS, no build step) that can create/list
products and create/look up orders against **either** backend — pick one from the cards at
the top of the page. Both APIs enable CORS (`AllowAnyOrigin`) for local development so the
page can call them directly from a different origin.

```bash
cd frontend
python3 -m http.server 8080   # or any static file server
```

Then open http://localhost:8080, with both APIs running (see above) in the background.

## Stack (both APIs)

- **.NET 10**
- **EF Core (SQLite)** for persistence, **EF Core InMemory** for tests
- **FluentValidation** for request validation
- **xUnit** for unit tests

## Testing

```bash
dotnet test
```

Runs unit tests for both APIs: handler/validator tests for the Vertical Slice API, and
service tests (against an in-memory database) for the Layered API.
