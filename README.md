# Vertical Slice Template

A .NET 10 Web API built with **Vertical Slice Architecture** — organized by feature
instead of by technical layer — using **ASP.NET Core Identity** for authentication and
role-based authorization. This repository is meant to be used as a **student template**:
clone it, read through `Features/Products/CreateProduct/` as the reference example, then
build your own features the same way.

```
Path            src/VerticalSliceTemplate
Default port    5010
```

## Quick start

Prerequisites: [.NET 10 SDK](https://dotnet.microsoft.com/download).

```bash
cd src/VerticalSliceTemplate
dotnet run   # http://localhost:5010
```

On first run in Development, the app applies EF Core migrations (`dotnet ef database
update` under the hood, via `Database.MigrateAsync()`) and seeds two demo accounts:

| Username | Password | Role |
|---|---|---|
| `admin` | `Passw0rd!` | Admin |
| `user` | `Passw0rd!` | *(none)* |

If you ever need to run migrations by hand (e.g. after adding your own entity):

```bash
cd src/VerticalSliceTemplate
dotnet ef migrations add <Name> --output-dir Data/Migrations
dotnet ef database update
```

## Structure

```
src/VerticalSliceTemplate/
├── Features/
│   ├── Products/
│   │   ├── CreateProduct/   (Request, Validator, Response, Endpoint — the reference slice)
│   │   ├── GetProduct/      (Request, Response, Endpoint)
│   │   └── ListProducts/    (Response, Endpoint — no Request, it takes no input)
│   ├── Orders/
│   │   ├── CreateOrder/     (Request, Validator, Response, Endpoint)
│   │   └── GetOrder/        (Request, Response, Endpoint)
│   ├── Auth/
│   │   ├── Login/           (Request, Validator, Response, Endpoint — AllowAnonymous)
│   │   └── Register/        (Request, Validator, Response, Endpoint — AllowAnonymous)
│   ├── Shared/
│   │   ├── Abstractions/    (NotFoundException, UnauthorizedException)
│   │   └── Security/        (JwtOptions, JwtTokenGenerator)
│   └── README.md            (deep dive on the vertical slice pattern itself)
├── Data/
│   ├── AppDbContext.cs      (IdentityDbContext<ApplicationUser> + Products/Orders)
│   ├── AppDbContextFactory.cs
│   ├── DbSeeder.cs          (seeds the Admin role + two demo accounts)
│   ├── Entities/            (Product, Order, OrderItem, ApplicationUser, Roles)
│   └── Migrations/
├── Program.cs
└── appsettings.json
```

See **[Features/README.md](src/VerticalSliceTemplate/Features/README.md)** for a full
walkthrough of the vertical slice pattern — the folder-per-feature convention, the 4-file
shape, and how a new slice gets picked up automatically with zero changes to `Program.cs`.

Want hands-on practice? **[OVNING-TodoApp-VerticalSlice.md](OVNING-TodoApp-VerticalSlice.md)**
is a step-by-step exercise (in Swedish) that walks through adding a whole new feature — a
Todo list — the same way Products and Orders are built here.

## Authentication

See **[Features/Auth/README.md](src/VerticalSliceTemplate/Features/Auth/README.md)** for
the full picture. In short:

- Every route requires a JWT bearer token, **except** `POST /api/auth/register` and
  `POST /api/auth/login`.
- Register, then log in, to get a token:

```bash
curl -X POST http://localhost:5010/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"username":"jane","email":"jane@example.com","password":"Passw0rd!"}'

curl -X POST http://localhost:5010/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"jane","password":"Passw0rd!"}'
# => { "token": "eyJ...", "expiresAtUtc": "...", "username": "jane", "roles": [] }

curl http://localhost:5010/api/products -H "Authorization: Bearer <token>"
```

- `POST /api/products` additionally requires the **Admin** role — log in as the seeded
  `admin` account (or promote a user yourself) to create a product. Any other authenticated
  user gets `403 Forbidden`.
- User storage, password hashing, and lockout are handled entirely by **ASP.NET Core
  Identity** (`UserManager`/`SignInManager`/`RoleManager`) — nothing is hand-rolled.
- The JWT signing key in `appsettings.json` is a **demo secret committed to the repo**.
  Replace it (e.g. with `dotnet user-secrets` or an environment variable) before using this
  outside a local demo.

## Adding a new feature

1. Create a folder under `Features/<Area>/<FeatureName>/`.
2. Add a `Request` record, a `Response` record, and — if the input needs validating — a
   `Validator` (`AbstractValidator<TRequest>`).
3. Add an `Endpoint` class deriving from `Endpoint<TRequest, TResponse>` (or
   `EndpointWithoutRequest<TResponse>` if there's no input). Set the route and any auth
   rules in `Configure()`, and put the actual logic in `HandleAsync` — there's no separate
   Handler class, the endpoint *is* the handler. Every endpoint requires a signed-in user
   by default; call `AllowAnonymous()` in `Configure()` if it shouldn't.
4. That's it — `AddFastEndpoints()`/`UseFastEndpoints()` (see `Program.cs`) scan the
   assembly for endpoints and validators at startup, so nothing else needs registering.
   `Features/Products/CreateProduct/` is the fastest way to see the shape — copy it as a
   starting point.

## Frontend (`frontend/`)

A dependency-free static page (plain HTML/CSS/JS, no build step) that can register, log in,
and then create/list products and create/look up orders against the API above. It's served
by a minimal ASP.NET Core static-file host, `Frontend.csproj`:

```bash
cd frontend
dotnet run   # http://localhost:8080
```

Then open http://localhost:8080, with the API (see above) running in the background. The
API enables CORS (`AllowAnyOrigin`) for local development so the page can call it directly
from a different origin. See **[frontend/README.md](frontend/README.md)** for details.

## Stack

- **.NET 10**
- **ASP.NET Core Identity** (`AddIdentityCore` + EF Core stores) for user/role management
- **JWT bearer authentication** (`Microsoft.AspNetCore.Authentication.JwtBearer`) issued
  after a successful Identity login
- **FastEndpoints** for routing — each feature's `Endpoint<TRequest, TResponse>` class
  handles its own request directly, no mediator layer in between
- **FluentValidation**, auto-discovered and run by FastEndpoints before `HandleAsync`
- **EF Core (SQLite)** for persistence, **EF Core InMemory** for tests
- **xUnit** for unit tests

## Testing

```bash
dotnet test
```

Covers endpoint and validator tests for every feature, including `Login`/`Register`
tests that exercise real `UserManager`/`SignInManager`/`RoleManager` instances against an
in-memory database (see `tests/VerticalSliceTemplate.Tests/Common/IdentityTestFactory.cs`)
rather than mocking Identity's manager classes. Endpoint tests use FastEndpoints'
`Factory.Create<TEndpoint>(...)` to instantiate an endpoint directly and call `HandleAsync`,
asserting on its `Response` — no `WebApplicationFactory` or real HTTP server needed.
