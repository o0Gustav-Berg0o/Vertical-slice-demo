# Authentication: ASP.NET Core Identity + JWT

This app uses **ASP.NET Core Identity** for everything about *who a user is* — accounts,
password hashing, lockout, roles — and its own thin JWT layer for *how a logged-in user
proves it* on later requests. This document explains how those two pieces fit together, and
why this template is built this way instead of hand-rolling authentication.

## Why Identity instead of hand-rolled auth

Writing your own password hashing, salt generation, and brute-force lockout is easy to get
subtly wrong, and easy to get right the first time and then forget to maintain. ASP.NET Core
Identity is battle-tested, actively maintained by Microsoft, and does all of this for you:

- **Password hashing** — `PasswordHasher<TUser>`, used internally by `UserManager`. Nothing
  in this codebase touches a raw password hash.
- **Account lockout** — configurable "too many failed attempts" protection (see
  `Program.cs`), enforced automatically by `SignInManager.CheckPasswordSignInAsync`.
- **Roles** — `RoleManager<IdentityRole>` and `UserManager.AddToRoleAsync`/`GetRolesAsync`,
  backed by real database tables (`AspNetRoles`, `AspNetUserRoles`) instead of a single
  string column you'd have to maintain and validate yourself.

## The pieces

- **`Data/Entities/ApplicationUser.cs`** — `class ApplicationUser : IdentityUser`. Extends
  Identity's built-in user type. Uses the default `string` (GUID) primary key rather than
  fighting Identity's defaults — see the comment on the class for why.
- **`Data/AppDbContext.cs`** — `class AppDbContext : IdentityDbContext<ApplicationUser>`.
  This one base class is what creates the `AspNetUsers`, `AspNetRoles`, `AspNetUserRoles`,
  `AspNetUserClaims`, etc. tables alongside `Products`/`Orders`/`OrderItems`.
- **`Program.cs`** — `AddIdentityCore<ApplicationUser>(...)` registers `UserManager`,
  configures the password and lockout policy, and (via `.AddRoles<IdentityRole>()`) enables
  role support. This is `AddIdentityCore`, **not** the full `AddIdentity` — the full version
  also wires up cookie authentication and external logins for browser-based MVC/Razor apps,
  none of which a pure API needs.
- **`Features/Shared/Security/JwtTokenGenerator.cs`** — issues an HMAC-SHA256-signed JWT
  containing the user's id, username, and one `ClaimTypes.Role` claim per role. This is
  separate from Identity itself: Identity has no built-in JWT support, so this template adds
  a small, explicit layer for it.
- **`Features/Auth/Login/`** and **`Features/Auth/Register/`** — ordinary vertical slices
  (see [../README.md](../README.md)), just like `Products/CreateProduct/`. Their endpoints
  use `UserManager`/`SignInManager` instead of talking to `AppDbContext` directly — that's
  what "using Identity" means in code.

## The request flow

**Register** (`POST /api/auth/register`, anonymous):
`RegisterEndpoint.HandleAsync` calls `UserManager.CreateAsync(user, password)`. New users get
**no role** — least privilege by default. If Identity rejects the password (or the username is
taken), its `IdentityResult.Errors` are translated into a FluentValidation `ValidationException`
so the same global exception handler that handles every other slice's validation failures
handles this one too, with no special-casing in `Program.cs`.

**Login** (`POST /api/auth/login`, anonymous):
1. `UserManager.FindByNameAsync(username)` looks the user up.
2. `SignInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true)` verifies
   the password *and* enforces the lockout policy — repeated failures lock the account for a
   while, independent of whether the correct password eventually gets used.
3. `UserManager.GetRolesAsync(user)` fetches the user's roles.
4. `JwtTokenGenerator.GenerateToken(user, roles)` embeds those roles as claims and signs the
   token.
5. The client receives `{ token, expiresAtUtc, username, roles }` and sends `token` back as
   `Authorization: Bearer <token>` on every later request.

**Every other request**: the JWT bearer middleware (configured in `Program.cs`) validates
the token's signature, issuer, audience, and expiry. If it's valid, `HttpContext.User`
carries the claims embedded at login — including the role claims — so a role check like
`Roles("Admin")` is a claim lookup against the token, not a database query.

## Protecting an endpoint

```csharp
public override void Configure()
{
    Post("/api/products");

    // Signed-in AND in the Admin role (see CreateProductEndpoint.cs):
    Roles(Roles.Admin);
}
```

```csharp
public override void Configure()
{
    Post("/api/auth/login");

    // No token required at all (only Login and Register use this):
    AllowAnonymous();
}
```

Every endpoint is secure-by-default — calling neither `Roles(...)` nor `AllowAnonymous()`
in `Configure()` means "any signed-in user, any role" (see `GetProductEndpoint.cs`).

`Roles.Admin` is a constant in `Data/Entities/Roles.cs` — use the constant, not a literal
`"Admin"` string, so a typo becomes a compile error instead of a silently-broken check.

## Demo accounts

`DbSeeder` seeds two accounts on startup (Development only):

| Username | Password | Role |
|---|---|---|
| `admin` | `Passw0rd!` | Admin |
| `user` | `Passw0rd!` | *(none)* |

Use `user` to see a `403 Forbidden` from `POST /api/products` firsthand, then `admin` to see
it succeed.

## A note on the signing key

The JWT signing key in `appsettings.json` (`Jwt:SigningKey`) is a demo secret committed to
this repo, so the template runs out of the box. **Replace it before deploying anything real**
— with [`dotnet user-secrets`](https://learn.microsoft.com/aspnet/core/security/app-secrets)
locally, or an environment variable / secret manager in any real environment. Anyone with
this key can forge a valid token.
