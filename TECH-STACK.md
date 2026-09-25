# Tech Stack

This is the stack you'll be building your own project in, based on this template.

## Backend (`src/VerticalSliceTemplate`)

| Technology | Role |
|---|---|
| **.NET 10** | Runtime and SDK |
| **ASP.NET Core Minimal APIs** | HTTP endpoints — no MVC controllers, each feature maps its own route directly |
| **MediatR** | In-process command/query dispatch — routes each request to exactly one handler |
| **FluentValidation** | Request validation, wired in as a MediatR pipeline behavior so it runs automatically before every handler |
| **Entity Framework Core (SQLite)** | Persistence — one `AppDbContext`, one SQLite file, migrations under `Data/Migrations/` |
| **ASP.NET Core Identity** (`AddIdentityCore`) | User accounts, password hashing, account lockout, and roles — via `UserManager`/`SignInManager`/`RoleManager` |
| **JWT Bearer authentication** | Stateless auth for API requests — a signed token is issued at login and validated on every later request |

## Architecture pattern

**Vertical Slice Architecture**: the app is organized by feature (`Features/Products/CreateProduct/`, `Features/Auth/Login/`, etc.), not by technical layer. Each feature folder is self-contained: a Command/Query, a Validator, a Handler, a Response, and an Endpoint. See [`src/VerticalSliceTemplate/Features/README.md`](src/VerticalSliceTemplate/Features/README.md) for the full explanation.

## Authentication

See [`src/VerticalSliceTemplate/Features/Auth/README.md`](src/VerticalSliceTemplate/Features/Auth/README.md) for how ASP.NET Core Identity and JWT fit together, and how role-based authorization (the `Admin` role example) works.

## Testing

| Technology | Role |
|---|---|
| **xUnit** | Test framework |
| **EF Core InMemory** | In-memory database for handler/validator tests — no real SQLite file needed |
| Real `UserManager`/`SignInManager`/`RoleManager` (via `IdentityTestFactory`) | Used instead of mocks for Identity-dependent tests, since those classes are impractical to mock directly |

## Frontend (`frontend/`)

| Technology | Role |
|---|---|
| **Plain HTML/CSS/JavaScript** | No framework, no build step |
| A minimal **ASP.NET Core** static-file host (`Frontend.csproj`) | Serves the HTML/CSS/JS files — `dotnet run` and nothing else |

## Tooling you'll need

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- The EF Core CLI tool, for creating/applying migrations:
  ```bash
  dotnet tool install --global dotnet-ef
  ```

## Where to look next

- [`README.md`](README.md) — quick start and how to run everything
- [`src/VerticalSliceTemplate/Features/README.md`](src/VerticalSliceTemplate/Features/README.md) — the vertical slice pattern, in depth
- [`src/VerticalSliceTemplate/Features/Auth/README.md`](src/VerticalSliceTemplate/Features/Auth/README.md) — authentication, in depth
- [`OVNING-TodoApp-VerticalSlice.md`](OVNING-TodoApp-VerticalSlice.md) — a hands-on exercise using this exact stack
