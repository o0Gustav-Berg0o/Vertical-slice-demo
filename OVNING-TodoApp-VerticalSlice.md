# Övning: bygg en Todo-funktion med Vertical Slice Architecture

Den här övningen går ut på att lägga till en helt ny funktion — **Todos** — i
`src/VerticalSliceTemplate`, byggd på exakt samma sätt som `Features/Products` och
`Features/Orders` redan är gjorda: en mapp per handling (slice), med sitt eget
`Request`, `Validator` och `Endpoint`, utan delade
"lager" (ingen `ProductService`, ingen `ProductRepository`).

Facit finns längst ner om du fastnar, men försök själv steg för steg först —
det är poängen med övningen.

## Mål

När du är klar ska du kunna:

- Logga in (`POST /api/auth/login`, samma som idag)
- `POST /api/todos` — skapa en todo (`{ "title": "Handla mjölk" }`)
- `GET /api/todos` — lista alla todos
- `GET /api/todos/{id}` — hämta en todo
- `PATCH /api/todos/{id}/complete` — markera en todo som klar
- `DELETE /api/todos/{id}` — ta bort en todo

Alla endpoints ska kräva inloggning. Det är standardbeteendet i FastEndpoints
(secure by default) — du behöver inte skriva någon kod för att kräva det, bara
undvika `AllowAnonymous()`, precis som Products och Orders gör idag.

## Innan du börjar — läs mönstret

Öppna en befintlig slice och läs igenom den, t.ex.
`src/VerticalSliceTemplate/Features/Products/CreateProduct/`:

| Fil | Ansvar |
|---|---|
| `CreateProductRequest.cs` | En vanlig `record` — inget basgränssnitt att implementera. FastEndpoints binder inkommande JSON direkt till den |
| `CreateProductValidator.cs` | En `AbstractValidator<T>` (FluentValidation) — körs automatiskt av FastEndpoints innan `HandleAsync` nås |
| `CreateProductResponse.cs` | Vad som skickas tillbaka som JSON |
| `CreateProductEndpoint.cs` | Ärver `Endpoint<TRequest, TResponse>`. `Configure()` sätter route och auth-regler; `HandleAsync` *är* affärslogiken — pratar direkt med `AppDbContext`, ingen separat Handler-klass |

`builder.Services.AddFastEndpoints(...)` och `app.UseFastEndpoints()` i
`Program.cs` scannar assemblyn vid uppstart och hittar automatiskt varje klass
som ärver från `Endpoint<,>`/`EndpointWithoutRequest<>`, plus varje
`AbstractValidator<T>` — **du behöver alltså inte registrera något manuellt i
`Program.cs`**. Skapa filerna på rätt ställe och ärv från rätt basklass, så
plockas de upp automatiskt.

## Steg 1 — Entiteten

Skapa `src/VerticalSliceTemplate/Data/Entities/Todo.cs`:

- `Id` (int)
- `Title` (string)
- `IsDone` (bool)
- `CreatedAt` (DateTime)

Jämför med `Data/Entities/Product.cs` för stil.

## Steg 2 — Registrera i `AppDbContext`

Öppna `src/VerticalSliceTemplate/Data/AppDbContext.cs`:

1. Lägg till `public DbSet<Todo> Todos => Set<Todo>();`
2. Lägg till en `modelBuilder.Entity<Todo>(entity => { ... })`-konfiguration i
   `OnModelCreating` — titta på hur `Product` konfigureras (`IsRequired()`,
   `HasMaxLength(200)`) och gör motsvarande för `Title`.

## Steg 3 — Mappstrukturen

Skapa under `src/VerticalSliceTemplate/Features/`:

```
Features/
└── Todos/
    ├── CreateTodo/
    ├── ListTodos/
    ├── GetTodo/
    ├── CompleteTodo/
    └── DeleteTodo/
```

Bygg nu varje slice, en i taget, i den ordningen (det är den enklaste
ordningen att testa i).

### 3a. `CreateTodo` — `POST /api/todos`

- `CreateTodoRequest(string Title)` — vanlig `record`
- `CreateTodoValidator` — `Title` får inte vara tomt, max 200 tecken
  (`RuleFor(x => x.Title).NotEmpty().MaximumLength(200);`)
- `CreateTodoResponse(int Id, string Title, bool IsDone, DateTime CreatedAt)`
- `CreateTodoEndpoint` — `Endpoint<CreateTodoRequest, CreateTodoResponse>`.
  I `Configure()`: `Post("/api/todos");`. I `HandleAsync`: skapa en `Todo`,
  sätt `CreatedAt = DateTime.UtcNow`, `db.Todos.Add(...)`,
  `await db.SaveChangesAsync(ct)`, och skicka svaret med
  `await Send.ResponseAsync(response, StatusCodes.Status201Created, ct);`
  (jämför med `CreateProductEndpoint`)

**Testa innan du går vidare** (se testavsnittet nedan) — bygg och gör ett
riktigt HTTP-anrop. Fortsätt inte förrän `POST /api/todos` faktiskt fungerar.

### 3b. `ListTodos` — `GET /api/todos`

- Inget Request behövs — jämför med `ListProductsEndpoint`
- `ListTodosResponse(int Id, string Title, bool IsDone)`
- `ListTodosEndpoint` — `EndpointWithoutRequest<List<ListTodosResponse>>`.
  `Configure()`: `Get("/api/todos");`. `HandleAsync(CancellationToken ct)`:
  `db.Todos.OrderBy(t => t.CreatedAt).Select(...).ToListAsync(ct)`, sedan
  `await Send.OkAsync(result, ct);`

### 3c. `GetTodo` — `GET /api/todos/{id}`

- `GetTodoRequest(int Id)`
- `GetTodoResponse(int Id, string Title, bool IsDone, DateTime CreatedAt)`
- `GetTodoEndpoint` — `Endpoint<GetTodoRequest, GetTodoResponse>`.
  `Configure()`: `Get("/api/todos/{id}");`. I `HandleAsync`: hitta via
  `FirstOrDefaultAsync`; om `null`, `await Send.NotFoundAsync(ct); return;`;
  annars `await Send.OkAsync(response, ct);` (jämför med `GetProductEndpoint`)

### 3d. `CompleteTodo` — `PATCH /api/todos/{id}/complete`

Det här är den enda slicen som inte har en direkt förebild bland Products/
Orders — testa att lösa den själv utifrån mönstret ovan.

- `CompleteTodoRequest(int Id)`
- `CompleteTodoResponse(int Id, string Title, bool IsDone)`
- `CompleteTodoEndpoint` — `Endpoint<CompleteTodoRequest, CompleteTodoResponse>`.
  `Configure()`: `Patch("/api/todos/{id}/complete");`. I `HandleAsync`: hämta
  todon, om `null` `await Send.NotFoundAsync(ct); return;`, annars sätt
  `todo.IsDone = true`, spara, `await Send.OkAsync(response, ct);`

### 3e. `DeleteTodo` — `DELETE /api/todos/{id}`

- `DeleteTodoRequest(int Id)`
- `DeleteTodoEndpoint` — ärv `Endpoint<DeleteTodoRequest>` (bara **en**
  generisk parameter — det finns inget svar att skicka tillbaka, bara en
  statuskod). `Configure()`: `Delete("/api/todos/{id}");`. I `HandleAsync`:
  hitta todon; om `null`, `await Send.NotFoundAsync(ct);`; annars
  `db.Todos.Remove(todo)`, spara, `await Send.NoContentAsync(ct);`
- Ingen validator behövs här — `Id` kommer från routen, inte från en body.

## Steg 4 — Migration (valfritt men rekommenderat)

Appen kör EF Core-migrationer automatiskt vid uppstart i Development
(`Database.MigrateAsync()`, se `Program.cs`), så för att `Todos`-tabellen ska
skapas måste du generera och ha med en migration:

```bash
cd src/VerticalSliceTemplate
dotnet ef migrations add AddTodos -o Data/Migrations
```

Om du redan har en lokal `verticalslicetemplate.db`-fil från tidigare körningar
(den är gitignorad, så den finns bara på din maskin) uppdateras den automatiskt
av migrationen nästa gång du kör `dotnet run` — ingen manuell radering behövs.

## Steg 5 — Bygg och testa

```bash
cd src/VerticalSliceTemplate
dotnet build
dotnet run   # http://localhost:5010
```

Logga in och spara token:

```bash
curl -s -X POST http://localhost:5010/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"Passw0rd!"}'
```

Kopiera `token`-värdet och testa varje endpoint (byt ut `<TOKEN>`):

```bash
# Skapa
curl -s -X POST http://localhost:5010/api/todos \
  -H "Content-Type: application/json" -H "Authorization: Bearer <TOKEN>" \
  -d '{"title":"Handla mjölk"}'

# Lista
curl -s http://localhost:5010/api/todos -H "Authorization: Bearer <TOKEN>"

# Hämta en
curl -s http://localhost:5010/api/todos/1 -H "Authorization: Bearer <TOKEN>"

# Markera klar
curl -s -X PATCH http://localhost:5010/api/todos/1/complete \
  -H "Authorization: Bearer <TOKEN>"

# Ta bort
curl -s -X DELETE http://localhost:5010/api/todos/1 \
  -H "Authorization: Bearer <TOKEN>" -o /dev/null -w "%{http_code}\n"
```

## Steg 6 — Tester (valfritt)

Titta på `tests/VerticalSliceTemplate.Tests/Features/Products/CreateProductEndpointTests.cs`
och `tests/VerticalSliceTemplate.Tests/Common/TestDbContextFactory.cs`. Skapa
motsvarande `tests/VerticalSliceTemplate.Tests/Features/Todos/`-mapp med ett
test per endpoint — instansiera endpointen med
`Factory.Create<CreateTodoEndpoint>(db)`, anropa `HandleAsync(request, ct)`
direkt och kolla `ep.Response`. Ett test som skapar en todo och kollar att den
sparades är en bra start. Kör med `dotnet test`.

## Steg 7 — (bonus) koppla in det i frontend

Om du vill gå längre: lägg till ett todo-avsnitt i
`frontend/vsa/index.html` + `frontend/shared/app.js`, med samma mönster som
produkt-formuläret (`apiFetch("/api/todos", { method: "POST", ... })`).
Det kräver ingen ändring i `frontend/shared/app.js`:s kärnlogik — bara ett
nytt formulär och en `fetch`-koppling i `setupForms()`.

---

## Facit

<details>
<summary>Klicka för att expandera lösningen</summary>

### `Data/Entities/Todo.cs`

```csharp
namespace VerticalSliceTemplate.Data.Entities;

public class Todo
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool IsDone { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

### `Data/AppDbContext.cs` (tillägg)

```csharp
public DbSet<Todo> Todos => Set<Todo>();

// i OnModelCreating:
modelBuilder.Entity<Todo>(entity =>
{
    entity.Property(t => t.Title).IsRequired().HasMaxLength(200);
});
```

### `Features/Todos/CreateTodo/`

```csharp
// CreateTodoRequest.cs
namespace VerticalSliceTemplate.Features.Todos.CreateTodo;

public record CreateTodoRequest(string Title);
```

```csharp
// CreateTodoValidator.cs
using FluentValidation;

namespace VerticalSliceTemplate.Features.Todos.CreateTodo;

public class CreateTodoValidator : AbstractValidator<CreateTodoRequest>
{
    public CreateTodoValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
    }
}
```

```csharp
// CreateTodoResponse.cs
namespace VerticalSliceTemplate.Features.Todos.CreateTodo;

public record CreateTodoResponse(int Id, string Title, bool IsDone, DateTime CreatedAt);
```

```csharp
// CreateTodoEndpoint.cs
using FastEndpoints;
using VerticalSliceTemplate.Data;
using VerticalSliceTemplate.Data.Entities;

namespace VerticalSliceTemplate.Features.Todos.CreateTodo;

public class CreateTodoEndpoint(AppDbContext db) : Endpoint<CreateTodoRequest, CreateTodoResponse>
{
    public override void Configure()
    {
        Post("/api/todos");
    }

    public override async Task HandleAsync(CreateTodoRequest req, CancellationToken ct)
    {
        var todo = new Todo
        {
            Title = req.Title,
            IsDone = false,
            CreatedAt = DateTime.UtcNow,
        };

        db.Todos.Add(todo);
        await db.SaveChangesAsync(ct);

        var response = new CreateTodoResponse(todo.Id, todo.Title, todo.IsDone, todo.CreatedAt);
        await Send.ResponseAsync(response, StatusCodes.Status201Created, ct);
    }
}
```

### `Features/Todos/ListTodos/`

```csharp
// ListTodosResponse.cs
namespace VerticalSliceTemplate.Features.Todos.ListTodos;

public record ListTodosResponse(int Id, string Title, bool IsDone);
```

```csharp
// ListTodosEndpoint.cs
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using VerticalSliceTemplate.Data;

namespace VerticalSliceTemplate.Features.Todos.ListTodos;

public class ListTodosEndpoint(AppDbContext db) : EndpointWithoutRequest<List<ListTodosResponse>>
{
    public override void Configure()
    {
        Get("/api/todos");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var todos = await db.Todos
            .OrderBy(t => t.CreatedAt)
            .Select(t => new ListTodosResponse(t.Id, t.Title, t.IsDone))
            .ToListAsync(ct);

        await Send.OkAsync(todos, ct);
    }
}
```

### `Features/Todos/GetTodo/`

```csharp
// GetTodoRequest.cs
namespace VerticalSliceTemplate.Features.Todos.GetTodo;

public record GetTodoRequest(int Id);
```

```csharp
// GetTodoResponse.cs
namespace VerticalSliceTemplate.Features.Todos.GetTodo;

public record GetTodoResponse(int Id, string Title, bool IsDone, DateTime CreatedAt);
```

```csharp
// GetTodoEndpoint.cs
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using VerticalSliceTemplate.Data;

namespace VerticalSliceTemplate.Features.Todos.GetTodo;

public class GetTodoEndpoint(AppDbContext db) : Endpoint<GetTodoRequest, GetTodoResponse>
{
    public override void Configure()
    {
        Get("/api/todos/{id}");
    }

    public override async Task HandleAsync(GetTodoRequest req, CancellationToken ct)
    {
        var todo = await db.Todos
            .Where(t => t.Id == req.Id)
            .Select(t => new GetTodoResponse(t.Id, t.Title, t.IsDone, t.CreatedAt))
            .FirstOrDefaultAsync(ct);

        if (todo is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(todo, ct);
    }
}
```

### `Features/Todos/CompleteTodo/`

```csharp
// CompleteTodoRequest.cs
namespace VerticalSliceTemplate.Features.Todos.CompleteTodo;

public record CompleteTodoRequest(int Id);
```

```csharp
// CompleteTodoResponse.cs
namespace VerticalSliceTemplate.Features.Todos.CompleteTodo;

public record CompleteTodoResponse(int Id, string Title, bool IsDone);
```

```csharp
// CompleteTodoEndpoint.cs
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using VerticalSliceTemplate.Data;

namespace VerticalSliceTemplate.Features.Todos.CompleteTodo;

public class CompleteTodoEndpoint(AppDbContext db) : Endpoint<CompleteTodoRequest, CompleteTodoResponse>
{
    public override void Configure()
    {
        Patch("/api/todos/{id}/complete");
    }

    public override async Task HandleAsync(CompleteTodoRequest req, CancellationToken ct)
    {
        var todo = await db.Todos.FirstOrDefaultAsync(t => t.Id == req.Id, ct);
        if (todo is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        todo.IsDone = true;
        await db.SaveChangesAsync(ct);

        await Send.OkAsync(new CompleteTodoResponse(todo.Id, todo.Title, todo.IsDone), ct);
    }
}
```

### `Features/Todos/DeleteTodo/`

```csharp
// DeleteTodoRequest.cs
namespace VerticalSliceTemplate.Features.Todos.DeleteTodo;

public record DeleteTodoRequest(int Id);
```

```csharp
// DeleteTodoEndpoint.cs
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using VerticalSliceTemplate.Data;

namespace VerticalSliceTemplate.Features.Todos.DeleteTodo;

// Endpoint<TRequest> (one generic argument) — there's nothing to send back
// besides a status code, so there's no TResponse.
public class DeleteTodoEndpoint(AppDbContext db) : Endpoint<DeleteTodoRequest>
{
    public override void Configure()
    {
        Delete("/api/todos/{id}");
    }

    public override async Task HandleAsync(DeleteTodoRequest req, CancellationToken ct)
    {
        var todo = await db.Todos.FirstOrDefaultAsync(t => t.Id == req.Id, ct);
        if (todo is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        db.Todos.Remove(todo);
        await db.SaveChangesAsync(ct);
        await Send.NoContentAsync(ct);
    }
}
```

</details>
