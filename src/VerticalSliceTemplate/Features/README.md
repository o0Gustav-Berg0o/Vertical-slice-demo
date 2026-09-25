# The Vertical Slice pattern

This document explains the convention every feature in this app follows. Read it once,
then use `Products/CreateProduct/` as your reference the next time you add a feature.

## Why "vertical slice"?

A traditional layered app is organized *horizontally*: all controllers together, all
services together, all repositories together. Adding one feature means touching several of
those layers, and the code for a single feature is scattered across the project.

A vertical slice app is organized by *feature* instead: everything one operation needs —
its input, its validation, its logic, its output, and its route — lives in one folder. There
is no shared `ProductService` or `ProductRepository` that every product-related operation
has to go through. `CreateProduct` and `GetProduct` don't share code with each other beyond
the `AppDbContext` and the cross-cutting infrastructure in `Shared/`.

The trade-off: some duplication between slices (e.g. `CreateProduct` and `GetProduct` both
know how to shape a `Product` into a response). In exchange, each feature is easy to find,
easy to read start-to-finish, and safe to change without fear of breaking an unrelated
feature that happened to share a layer with it.

## The five-file shape

Look at `Products/CreateProduct/` — every write-operation slice in this app has this shape
(a read-only query slice like `GetProduct/` skips the Validator when there's nothing to
validate):

| File | Role |
|---|---|
| `CreateProductCommand.cs` | The input, as a `record` implementing `IRequest<TResponse>`. This is the "message" MediatR routes to a handler. |
| `CreateProductValidator.cs` | An `AbstractValidator<TCommand>` (FluentValidation). Found automatically and run automatically before the handler — see [ValidationBehavior](#the-validation-pipeline) below. |
| `CreateProductHandler.cs` | An `IRequestHandler<TCommand, TResponse>`. The actual business logic — talks to `AppDbContext` directly. |
| `CreateProductResponse.cs` | The shape returned to the caller as JSON. Kept separate from the EF Core entity so the API contract can evolve independently of the database schema. |
| `CreateProductEndpoint.cs` | Implements `IEndpoint`, maps one route with `app.MapPost(...)` (or `MapGet`/`MapDelete`/etc.), and forwards to MediatR with `sender.Send(command)`. |

## Auto-registration: nothing to wire up in `Program.cs`

Two extension methods in `Shared/Extensions/` do all the wiring, once, at startup:

- **`ServiceCollectionExtensions.AddFeatures`** scans the assembly and registers every
  `IRequestHandler<,>` (for MediatR) and every `AbstractValidator<T>` (for FluentValidation)
  it finds, plus the `ValidationBehavior` pipeline step (see below).
- **`WebApplicationExtensions.MapFeatureEndpoints`** scans the assembly for every
  non-abstract class implementing `IEndpoint` and calls its static `MapEndpoint(app)`.

This is why adding a feature never means editing `Program.cs`: create the five files in a
new folder under `Features/`, implement the right interfaces, and they're picked up the next
time the app starts.

`IEndpoint` uses a C# "static abstract interface member" (`static abstract void
MapEndpoint(IEndpointRouteBuilder app)`), which is what lets `MapFeatureEndpoints` call each
endpoint class's mapping method via reflection without ever constructing an instance of it —
endpoint classes exist purely to hold that one static method.

## The validation pipeline

`ValidationBehavior<TRequest, TResponse>` is a MediatR "pipeline behavior" — it wraps every
call to `ISender.Send(...)`. If the command/query being sent has a registered
`IValidator<TRequest>` (i.e. you wrote a `*Validator.cs` for it), `ValidationBehavior` runs
it and throws FluentValidation's `ValidationException` on failure, before the handler's
`Handle()` method is ever called. A global exception handler in `Program.cs` turns that
exception into a `400` response with field-level error details.

This means a handler never needs to check `if (!ModelState.IsValid)` or call a validator
itself — by the time `Handle()` runs, the input is already known-valid.

## Protecting an endpoint

Every `*Endpoint.cs` in this app calls one of:

- `.RequireAuthorization()` — any signed-in user (any valid bearer token).
- `.RequireAuthorization(policy => policy.RequireRole("Admin"))` — signed in **and** in a
  specific role (see `CreateProductEndpoint.cs`).
- `.AllowAnonymous()` — reachable without a token at all (only `Login`/`Register`).

See [Features/Auth/README.md](Auth/README.md) for how roles and tokens actually work.

## Adding your own feature

1. Create `Features/<Area>/<FeatureName>/`.
2. Copy the five-file shape from `Products/CreateProduct/`, renaming as you go.
3. Decide the route and HTTP method in the new `*Endpoint.cs`, and whether it needs
   `.RequireAuthorization()`, a role, or `.AllowAnonymous()`.
4. If the feature needs a new entity, add it under `Data/Entities/`, register a `DbSet<T>`
   and any `OnModelCreating` configuration in `Data/AppDbContext.cs`, then generate a
   migration: `dotnet ef migrations add <Name> --output-dir Data/Migrations`.
5. Run the app — no other registration is needed.

Want to practice this end to end? See
[OVNING-TodoApp-VerticalSlice.md](../../../OVNING-TodoApp-VerticalSlice.md) at the repo root.
