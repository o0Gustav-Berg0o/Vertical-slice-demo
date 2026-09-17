# Övning: bygg en Todo-funktion med Vertical Slice Architecture

Den här övningen går ut på att lägga till en helt ny funktion — **Todos** — i
`src/VerticalSliceDemo`, byggd på exakt samma sätt som `Features/Products` och
`Features/Orders` redan är gjorda: en mapp per handling (slice), med sitt eget
`Command`/`Query`, `Handler`, `Validator` och `Endpoint`, utan delade
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

Alla endpoints ska kräva inloggning (`RequireAuthorization()`), precis som
Products och Orders gör idag.

## Innan du börjar — läs mönstret

Öppna en befintlig slice och läs igenom den, t.ex.
`src/VerticalSliceDemo/Features/Products/CreateProduct/`:

| Fil | Ansvar |
|---|---|
| `CreateProductCommand.cs` | En `record` som implementerar `IRequest<TResponse>` — själva "meddelandet" |
| `CreateProductValidator.cs` | En `AbstractValidator<T>` (FluentValidation) — körs automatiskt av `ValidationBehavior` innan handlern nås |
| `CreateProductHandler.cs` | En `IRequestHandler<TCommand, TResponse>` — affärslogiken, pratar direkt med `AppDbContext` |
| `CreateProductResponse.cs` | Vad som skickas tillbaka som JSON |
| `CreateProductEndpoint.cs` | Implementerar `IEndpoint`, mappar en route med `app.MapPost(...)`, skickar requestet vidare med `sender.Send(command)` |

`Features/Shared/Extensions/ServiceCollectionExtensions.cs` och
`WebApplicationExtensions.cs` scannar assemblyn med reflection och
registrerar/mappar automatiskt allt som implementerar `IRequestHandler`,
`AbstractValidator` respektive `IEndpoint` — **du behöver alltså inte
registrera något manuellt i `Program.cs`**. Skapa filerna på rätt ställe
med rätt gränssnitt, så plockas de upp automatiskt.

## Steg 1 — Entiteten

Skapa `src/VerticalSliceDemo/Data/Entities/Todo.cs`:

- `Id` (int)
- `Title` (string)
- `IsDone` (bool)
- `CreatedAt` (DateTime)

Jämför med `Data/Entities/Product.cs` för stil.

## Steg 2 — Registrera i `AppDbContext`

Öppna `src/VerticalSliceDemo/Data/AppDbContext.cs`:

1. Lägg till `public DbSet<Todo> Todos => Set<Todo>();`
2. Lägg till en `modelBuilder.Entity<Todo>(entity => { ... })`-konfiguration i
   `OnModelCreating` — titta på hur `Product` konfigureras (`IsRequired()`,
   `HasMaxLength(200)`) och gör motsvarande för `Title`.

## Steg 3 — Mappstrukturen

Skapa under `src/VerticalSliceDemo/Features/`:

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

- `CreateTodoCommand(string Title) : IRequest<CreateTodoResponse>`
- `CreateTodoValidator` — `Title` får inte vara tomt, max 200 tecken
  (`RuleFor(x => x.Title).NotEmpty().MaximumLength(200);`)
- `CreateTodoResponse(int Id, string Title, bool IsDone, DateTime CreatedAt)`
- `CreateTodoHandler` — skapar en `Todo`, sätter `CreatedAt = DateTime.UtcNow`,
  `db.Todos.Add(...)`, `await db.SaveChangesAsync(...)`, returnerar en
  `CreateTodoResponse`
- `CreateTodoEndpoint` — `app.MapPost("/api/todos", ...)`, kom ihåg
  `.RequireAuthorization()` och `Results.Created($"/api/todos/{result.Id}", result)`
  (jämför med `CreateProductEndpoint`)

**Testa innan du går vidare** (se testavsnittet nedan) — bygg och gör ett
riktigt HTTP-anrop. Fortsätt inte förrän `POST /api/todos` faktiskt fungerar.

### 3b. `ListTodos` — `GET /api/todos`

- `ListTodosQuery : IRequest<IReadOnlyList<ListTodosResponse>>` (ingen data
  behövs — jämför med `ListProductsQuery`)
- `ListTodosResponse(int Id, string Title, bool IsDone)`
- `ListTodosHandler` — `db.Todos.OrderBy(t => t.CreatedAt).Select(...).ToListAsync(...)`
- `ListTodosEndpoint` — `app.MapGet("/api/todos", ...)`

### 3c. `GetTodo` — `GET /api/todos/{id}`

- `GetTodoQuery(int Id) : IRequest<GetTodoResponse?>`
- `GetTodoResponse(int Id, string Title, bool IsDone, DateTime CreatedAt)`
- `GetTodoHandler` — hitta via `FirstOrDefaultAsync`, returnera `null` om den
  inte finns
- `GetTodoEndpoint` — `app.MapGet("/api/todos/{id:int}", ...)`, returnera
  `Results.NotFound()` om resultatet är `null` (jämför med `GetProductEndpoint`)

### 3d. `CompleteTodo` — `PATCH /api/todos/{id}/complete`

Det här är den enda slicen som inte har en direkt förebild bland Products/
Orders — testa att lösa den själv utifrån mönstret ovan.

- `CompleteTodoCommand(int Id) : IRequest<CompleteTodoResponse?>`
- `CompleteTodoResponse(int Id, string Title, bool IsDone)`
- `CompleteTodoHandler` — hämta todon, om `null` returnera `null`, annars
  sätt `todo.IsDone = true`, spara, returnera svaret
- `CompleteTodoEndpoint` — `app.MapPatch("/api/todos/{id:int}/complete", ...)`
  (obs: `MapPatch`, inte `MapPost`), `Results.NotFound()` om `null`

### 3e. `DeleteTodo` — `DELETE /api/todos/{id}`

- `DeleteTodoCommand(int Id) : IRequest<bool>` (`true` = togs bort, `false` =
  fanns inte)
- `DeleteTodoHandler` — hitta todon; om `null`, returnera `false`; annars
  `db.Todos.Remove(todo)`, spara, returnera `true`
- `DeleteTodoEndpoint` — `app.MapDelete("/api/todos/{id:int}", ...)`,
  `Results.NoContent()` vid `true`, `Results.NotFound()` vid `false`
- Ingen validator behövs här — `Id` kommer från routen, inte från en body.

## Steg 4 — Migration (valfritt men rekommenderat)

Appen kör `db.Database.EnsureCreated()` vid uppstart (se `Program.cs`), så
en ny lokal databas skapas automatiskt med `Todos`-tabellen även utan
migration. Men för att följa samma konvention som resten av repot
(`Data/Migrations/`), skapa en migration ändå:

```bash
cd src/VerticalSliceDemo
dotnet ef migrations add AddTodos -o Data/Migrations
```

Om du redan har en lokal `verticalslicedemo.db`-fil från tidigare körningar
(den är gitignorad, så den finns bara på din maskin) måste du radera den
innan nästa `dotnet run`, annars saknar den gamla filen `Todos`-tabellen:

```bash
rm src/VerticalSliceDemo/verticalslicedemo.db
```

## Steg 5 — Bygg och testa

```bash
cd src/VerticalSliceDemo
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

Titta på `tests/VerticalSliceDemo.Tests/Features/Products/CreateProductHandlerTests.cs`
och `tests/VerticalSliceDemo.Tests/Common/TestDbContextFactory.cs`. Skapa
motsvarande `tests/VerticalSliceDemo.Tests/Features/Todos/`-mapp med ett
test per handler — ett test som skapar en todo och kollar att den sparades
är en bra start. Kör med `dotnet test`.

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
namespace VerticalSliceDemo.Data.Entities;

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
// CreateTodoCommand.cs
using MediatR;

namespace VerticalSliceDemo.Features.Todos.CreateTodo;

public record CreateTodoCommand(string Title) : IRequest<CreateTodoResponse>;
```

```csharp
// CreateTodoValidator.cs
using FluentValidation;

namespace VerticalSliceDemo.Features.Todos.CreateTodo;

public class CreateTodoValidator : AbstractValidator<CreateTodoCommand>
{
    public CreateTodoValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
    }
}
```

```csharp
// CreateTodoResponse.cs
namespace VerticalSliceDemo.Features.Todos.CreateTodo;

public record CreateTodoResponse(int Id, string Title, bool IsDone, DateTime CreatedAt);
```

```csharp
// CreateTodoHandler.cs
using MediatR;
using VerticalSliceDemo.Data;
using VerticalSliceDemo.Data.Entities;

namespace VerticalSliceDemo.Features.Todos.CreateTodo;

public class CreateTodoHandler(AppDbContext db) : IRequestHandler<CreateTodoCommand, CreateTodoResponse>
{
    public async Task<CreateTodoResponse> Handle(CreateTodoCommand request, CancellationToken cancellationToken)
    {
        var todo = new Todo
        {
            Title = request.Title,
            IsDone = false,
            CreatedAt = DateTime.UtcNow,
        };

        db.Todos.Add(todo);
        await db.SaveChangesAsync(cancellationToken);

        return new CreateTodoResponse(todo.Id, todo.Title, todo.IsDone, todo.CreatedAt);
    }
}
```

```csharp
// CreateTodoEndpoint.cs
using MediatR;
using VerticalSliceDemo.Features.Shared.Abstractions;

namespace VerticalSliceDemo.Features.Todos.CreateTodo;

public class CreateTodoEndpoint : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/todos", async (CreateTodoCommand command, ISender sender) =>
            {
                var result = await sender.Send(command);
                return Results.Created($"/api/todos/{result.Id}", result);
            })
            .WithName("CreateTodo")
            .WithTags("Todos")
            .RequireAuthorization()
            .Produces<CreateTodoResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem();
    }
}
```

### `Features/Todos/ListTodos/`

```csharp
// ListTodosQuery.cs
using MediatR;

namespace VerticalSliceDemo.Features.Todos.ListTodos;

public record ListTodosQuery : IRequest<IReadOnlyList<ListTodosResponse>>;
```

```csharp
// ListTodosResponse.cs
namespace VerticalSliceDemo.Features.Todos.ListTodos;

public record ListTodosResponse(int Id, string Title, bool IsDone);
```

```csharp
// ListTodosHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using VerticalSliceDemo.Data;

namespace VerticalSliceDemo.Features.Todos.ListTodos;

public class ListTodosHandler(AppDbContext db) : IRequestHandler<ListTodosQuery, IReadOnlyList<ListTodosResponse>>
{
    public async Task<IReadOnlyList<ListTodosResponse>> Handle(ListTodosQuery request, CancellationToken cancellationToken)
    {
        return await db.Todos
            .OrderBy(t => t.CreatedAt)
            .Select(t => new ListTodosResponse(t.Id, t.Title, t.IsDone))
            .ToListAsync(cancellationToken);
    }
}
```

```csharp
// ListTodosEndpoint.cs
using MediatR;
using VerticalSliceDemo.Features.Shared.Abstractions;

namespace VerticalSliceDemo.Features.Todos.ListTodos;

public class ListTodosEndpoint : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/todos", async (ISender sender) =>
            {
                var result = await sender.Send(new ListTodosQuery());
                return Results.Ok(result);
            })
            .WithName("ListTodos")
            .WithTags("Todos")
            .RequireAuthorization()
            .Produces<IReadOnlyList<ListTodosResponse>>();
    }
}
```

### `Features/Todos/GetTodo/`

```csharp
// GetTodoQuery.cs
using MediatR;

namespace VerticalSliceDemo.Features.Todos.GetTodo;

public record GetTodoQuery(int Id) : IRequest<GetTodoResponse?>;
```

```csharp
// GetTodoResponse.cs
namespace VerticalSliceDemo.Features.Todos.GetTodo;

public record GetTodoResponse(int Id, string Title, bool IsDone, DateTime CreatedAt);
```

```csharp
// GetTodoHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using VerticalSliceDemo.Data;

namespace VerticalSliceDemo.Features.Todos.GetTodo;

public class GetTodoHandler(AppDbContext db) : IRequestHandler<GetTodoQuery, GetTodoResponse?>
{
    public async Task<GetTodoResponse?> Handle(GetTodoQuery request, CancellationToken cancellationToken)
    {
        return await db.Todos
            .Where(t => t.Id == request.Id)
            .Select(t => new GetTodoResponse(t.Id, t.Title, t.IsDone, t.CreatedAt))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
```

```csharp
// GetTodoEndpoint.cs
using MediatR;
using VerticalSliceDemo.Features.Shared.Abstractions;

namespace VerticalSliceDemo.Features.Todos.GetTodo;

public class GetTodoEndpoint : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/todos/{id:int}", async (int id, ISender sender) =>
            {
                var result = await sender.Send(new GetTodoQuery(id));
                return result is not null ? Results.Ok(result) : Results.NotFound();
            })
            .WithName("GetTodo")
            .WithTags("Todos")
            .RequireAuthorization()
            .Produces<GetTodoResponse>()
            .Produces(StatusCodes.Status404NotFound);
    }
}
```

### `Features/Todos/CompleteTodo/`

```csharp
// CompleteTodoCommand.cs
using MediatR;

namespace VerticalSliceDemo.Features.Todos.CompleteTodo;

public record CompleteTodoCommand(int Id) : IRequest<CompleteTodoResponse?>;
```

```csharp
// CompleteTodoResponse.cs
namespace VerticalSliceDemo.Features.Todos.CompleteTodo;

public record CompleteTodoResponse(int Id, string Title, bool IsDone);
```

```csharp
// CompleteTodoHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using VerticalSliceDemo.Data;

namespace VerticalSliceDemo.Features.Todos.CompleteTodo;

public class CompleteTodoHandler(AppDbContext db) : IRequestHandler<CompleteTodoCommand, CompleteTodoResponse?>
{
    public async Task<CompleteTodoResponse?> Handle(CompleteTodoCommand request, CancellationToken cancellationToken)
    {
        var todo = await db.Todos.FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);
        if (todo is null)
        {
            return null;
        }

        todo.IsDone = true;
        await db.SaveChangesAsync(cancellationToken);

        return new CompleteTodoResponse(todo.Id, todo.Title, todo.IsDone);
    }
}
```

```csharp
// CompleteTodoEndpoint.cs
using MediatR;
using VerticalSliceDemo.Features.Shared.Abstractions;

namespace VerticalSliceDemo.Features.Todos.CompleteTodo;

public class CompleteTodoEndpoint : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch("/api/todos/{id:int}/complete", async (int id, ISender sender) =>
            {
                var result = await sender.Send(new CompleteTodoCommand(id));
                return result is not null ? Results.Ok(result) : Results.NotFound();
            })
            .WithName("CompleteTodo")
            .WithTags("Todos")
            .RequireAuthorization()
            .Produces<CompleteTodoResponse>()
            .Produces(StatusCodes.Status404NotFound);
    }
}
```

### `Features/Todos/DeleteTodo/`

```csharp
// DeleteTodoCommand.cs
using MediatR;

namespace VerticalSliceDemo.Features.Todos.DeleteTodo;

public record DeleteTodoCommand(int Id) : IRequest<bool>;
```

```csharp
// DeleteTodoHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using VerticalSliceDemo.Data;

namespace VerticalSliceDemo.Features.Todos.DeleteTodo;

public class DeleteTodoHandler(AppDbContext db) : IRequestHandler<DeleteTodoCommand, bool>
{
    public async Task<bool> Handle(DeleteTodoCommand request, CancellationToken cancellationToken)
    {
        var todo = await db.Todos.FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);
        if (todo is null)
        {
            return false;
        }

        db.Todos.Remove(todo);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
```

```csharp
// DeleteTodoEndpoint.cs
using MediatR;
using VerticalSliceDemo.Features.Shared.Abstractions;

namespace VerticalSliceDemo.Features.Todos.DeleteTodo;

public class DeleteTodoEndpoint : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/todos/{id:int}", async (int id, ISender sender) =>
            {
                var deleted = await sender.Send(new DeleteTodoCommand(id));
                return deleted ? Results.NoContent() : Results.NotFound();
            })
            .WithName("DeleteTodo")
            .WithTags("Todos")
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);
    }
}
```

</details>
