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

## The four-file shape

Look at `Products/CreateProduct/` — every write-operation slice in this app has this shape
(a read-only query slice like `GetProduct/` skips the Validator when there's nothing to
validate, and a slice with no input at all, like `ListProducts/`, skips the Request too):

| File | Role |
|---|---|
| `CreateProductRequest.cs` | The input, as a plain `record`. FastEndpoints binds the incoming JSON body (and route/query values) straight onto it — no base interface to implement. |
| `CreateProductValidator.cs` | An `AbstractValidator<TRequest>` (FluentValidation). Found and run automatically before `HandleAsync` — see [The validation pipeline](#the-validation-pipeline) below. |
| `CreateProductResponse.cs` | The shape returned to the caller as JSON. Kept separate from the EF Core entity so the API contract can evolve independently of the database schema. |
| `CreateProductEndpoint.cs` | An `Endpoint<TRequest, TResponse>` (or `EndpointWithoutRequest<TResponse>` when there's no input). `Configure()` sets the route and auth rules; `HandleAsync` *is* the business logic — there's no separate handler class to forward to. |

## Auto-registration: nothing to wire up in `Program.cs`

Two calls in `Program.cs` do all the wiring, once, at startup:

- **`builder.Services.AddFastEndpoints(o => o.IncludeAbstractValidators = true)`** scans the
  assembly and registers every class deriving from `Endpoint<,>`/`EndpointWithoutRequest<>`,
  plus every plain `AbstractValidator<T>` it finds (that's what
  `IncludeAbstractValidators = true` does — without it, FastEndpoints expects its own
  `Validator<T>` base class instead of FluentValidation's).
- **`app.UseFastEndpoints()`** maps each discovered endpoint's route.

This is why adding a feature never means editing `Program.cs`: create the files in a new
folder under `Features/`, derive from the right FastEndpoints base class, and they're picked
up the next time the app starts.

## The validation pipeline

FastEndpoints runs each endpoint's validator itself, before `HandleAsync` is ever called: if
the incoming `TRequest` has a registered `AbstractValidator<TRequest>` (i.e. you wrote a
`*Validator.cs` for it), a failure short-circuits the request straight into FluentValidation's
`ValidationException`, which the global exception handler in `Program.cs` turns into a `400`
response with field-level error details.

This means `HandleAsync` never needs to check `if (!ModelState.IsValid)` or call a validator
itself — by the time it runs, the input is already known-valid.

## Protecting an endpoint

Every endpoint in this app is **secure by default** — FastEndpoints requires a valid bearer
token on every route unless the endpoint's `Configure()` says otherwise:

- No extra call — any signed-in user (any valid bearer token). This is the default for most
  endpoints in this app.
- `Roles("Admin")` — signed in **and** in a specific role (see `CreateProductEndpoint.cs`).
- `AllowAnonymous()` — reachable without a token at all (only `Login`/`Register`).

See [Features/Auth/README.md](Auth/README.md) for how roles and tokens actually work.

## Adding your own feature

1. Create `Features/<Area>/<FeatureName>/`.
2. Copy the four-file shape from `Products/CreateProduct/`, renaming as you go.
3. Decide the route and HTTP method in the new `*Endpoint.cs`'s `Configure()`, and whether
   it needs `Roles(...)` or `AllowAnonymous()` (it's authenticated-only by default).
4. If the feature needs a new entity, add it under `Data/Entities/`, register a `DbSet<T>`
   and any `OnModelCreating` configuration in `Data/AppDbContext.cs`, then generate a
   migration: `dotnet ef migrations add <Name> --output-dir Data/Migrations`.
5. Run the app — no other registration is needed.

Want to practice this end to end? See
[OVNING-TodoApp-VerticalSlice.md](../../../OVNING-TodoApp-VerticalSlice.md) at the repo root.
