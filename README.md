# Architecture Comparison Demo

Two .NET 10 Web APIs implementing the **identical domain and HTTP contract** (Products,
Orders) with two different architectural styles, each with its own dedicated frontend —
so you can compare the styles side by side without the API surface changing.

| | Vertical Slice API | Layered API |
|---|---|---|
| Path | `src/VerticalSliceDemo` | `src/LayeredArchitectureDemo` |
| Organized by | Feature | Technical layer |
| Request dispatch | MediatR command/query | ASP.NET controllers |
| Validation | FluentValidation as a MediatR pipeline behavior | FluentValidation invoked explicitly in the service layer |
| Default port | 5010 | 5136 |

Both expose the same routes (`GET/POST /api/products`, `GET /api/products/{id}`,
`POST/GET /api/orders[/{id}]`) with the same JSON shapes, backed by their own SQLite database.
All of those routes require a **JWT bearer token** — see [Authentication](#authentication) below.

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
│   ├── Shared/
│   │   ├── Abstractions/    (IEndpoint, ValidationBehavior, NotFoundException, UnauthorizedException)
│   │   ├── Extensions/      (DI + endpoint-mapping helpers)
│   │   └── Security/        (PasswordHasher, JwtOptions, JwtTokenGenerator)
│   └── Auth/
│       └── Login/           (Command, Handler, Validator, Response, Endpoint — AllowAnonymous)
├── Data/
│   ├── AppDbContext.cs
│   ├── AppDbContextFactory.cs   (EF Core design-time factory)
│   ├── DbSeeder.cs              (seeds one demo user on startup)
│   ├── Entities/                (Product, Order, OrderItem, User)
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

Want hands-on practice with the pattern? [OVNING-TodoApp-VSA.md](OVNING-TodoApp-VSA.md)
is a step-by-step exercise (in Swedish) that walks through adding a whole new
feature — a Todo list — the same way Products and Orders are built here.

## 2. Layered API (`src/LayeredArchitectureDemo`)

The same domain, organized the traditional way: by technical layer instead of by feature.

```
src/LayeredArchitectureDemo/
├── Controllers/       (ProductsController, OrdersController [Authorize]; AuthController [AllowAnonymous])
├── Services/          (Product/Order/Auth services — business logic)
├── Repositories/      (Product/Order/User repositories — data access)
├── Models/
│   ├── Dtos/           (request/response records, incl. Login)
│   └── Entities/       (EF Core entities, incl. User)
├── Validators/         (FluentValidation validators for request DTOs)
├── Data/
│   ├── AppDbContext.cs
│   ├── AppDbContextFactory.cs
│   ├── DbSeeder.cs     (seeds one demo user on startup)
│   └── Migrations/
├── Common/             (NotFoundException, UnauthorizedException, PasswordHasher, JwtOptions, JwtTokenGenerator)
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

## Authentication

Both APIs require a JWT bearer token on every Products/Orders route. Each seeds one demo
user on startup and exposes an anonymous login endpoint that issues a token:

```bash
curl -X POST http://localhost:5010/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"Passw0rd!"}'
# => { "token": "eyJ...", "expiresAtUtc": "...", "username": "admin" }

curl http://localhost:5010/api/products -H "Authorization: Bearer <token>"
```

Notes:
- Passwords are hashed with PBKDF2 (100,000 iterations, per-user salt) — see `PasswordHasher`
  in each API's `Common`/`Features/Shared/Security` folder.
- Tokens are HS256-signed with a per-API secret configured under `Jwt:SigningKey` in each
  `appsettings.json`. **These are demo secrets committed to the repo** — replace them (e.g.
  with user secrets or an environment variable) before using this outside a local demo.
- A token issued by one API is not valid on the other — they have independent signing keys
  and issuers, even though the DTO shapes match.
- Requests without a valid token get `401 Unauthorized`; a failed login also returns `401`.

## 3. Frontends (`frontend/`)

Two dependency-free static pages (plain HTML/CSS/JS, no build step), one per backend, each
hard-wired to its own API so there's no switcher and no ambiguity about which backend a
click hits. They're served by a minimal ASP.NET Core static-file host, `Frontend.csproj`,
which is itself a project in the solution:

```
frontend/
├── Frontend.csproj   (static-file host — no build step for the HTML/CSS/JS themselves)
├── index.html        (landing page linking to vsa/ and layered/)
├── shared/            (styles.css + app.js — the UI logic, parameterized by window.BACKEND)
├── vsa/               (index.html for the Vertical Slice API, http://localhost:5010)
└── layered/           (index.html for the Layered API, http://localhost:5136)
```

Each page can log in, then create/list products and create/look up orders against its one
backend. The demo credentials are pre-filled in the login form (`admin`/`Passw0rd!` for the
Admin role, or `user`/`Passw0rd!` for the User role — creating a product requires Admin).
Both APIs enable CORS (`AllowAnyOrigin`) for local development so the pages can call them
directly from a different origin.

```bash
cd frontend
dotnet run   # http://localhost:8080
```

Then open http://localhost:8080/vsa/ or http://localhost:8080/layered/ (or
http://localhost:8080/ for a landing page with links to both), with the corresponding
API running (see above) in the background.

## Running all three together

All three projects (`VerticalSliceDemo`, `LayeredArchitectureDemo`, `Frontend`) are in
`VerticalSliceDemo.slnx`, and a shared multi-startup launch profile,
`VerticalSliceDemo.slnLaunch`, is checked in. In Visual Studio, open the solution, pick
**All (both APIs + frontend)** from the startup-project dropdown in the toolbar, and press
**F5** — both APIs start under the debugger and the frontend host starts alongside them
(opening your browser at http://localhost:8080).

From the CLI, start each in its own terminal:

```bash
cd src/VerticalSliceDemo && dotnet run        # http://localhost:5010
cd src/LayeredArchitectureDemo && dotnet run  # http://localhost:5136
cd frontend && dotnet run                     # http://localhost:8080
```

## Stack (both APIs)

- **.NET 10**
- **EF Core (SQLite)** for persistence, **EF Core InMemory** for tests
- **FluentValidation** for request validation
- **JWT bearer authentication** (`Microsoft.AspNetCore.Authentication.JwtBearer`)
- **xUnit** for unit tests

## Testing

```bash
dotnet test
```

Runs unit tests for both APIs: handler/validator/login tests for the Vertical Slice API, and
service tests, including auth, (against an in-memory database) for the Layered API.
