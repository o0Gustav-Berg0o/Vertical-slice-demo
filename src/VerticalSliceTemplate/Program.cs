using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using VerticalSliceTemplate.Data;
using VerticalSliceTemplate.Data.Entities;
using VerticalSliceTemplate.Features.Shared.Abstractions;
using VerticalSliceTemplate.Features.Shared.Extensions;
using VerticalSliceTemplate.Features.Shared.Security;

var builder = WebApplication.CreateBuilder(args);

// The database: SQLite via EF Core, shared by every feature slice and by
// ASP.NET Core Identity (see AppDbContext, which extends IdentityDbContext).
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? "Data Source=verticalslicetemplate.db"));

// Scans this assembly for MediatR handlers, FluentValidation validators, and
// wires the ValidationBehavior pipeline — see ServiceCollectionExtensions.
builder.Services.AddFeatures(typeof(Program).Assembly);

// ASP.NET Core Identity, "Core" flavor: AddIdentityCore (not the full AddIdentity)
// gives us UserManager/SignInManager/RoleManager and EF-Core-backed user/role
// storage, but skips the cookie-auth/external-login/UI pieces meant for MVC/Razor
// apps with browser sessions — this is a pure API, so none of that applies.
// This is what "using .NET's own Identity" means: real, tested password hashing
// and lockout instead of a hand-rolled PBKDF2 implementation.
builder.Services.AddIdentityCore<ApplicationUser>(options =>
    {
        // Configured explicitly (not left at Identity's hidden defaults) so the
        // policy is visible here and easy to tune.
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = false;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        options.User.RequireUniqueEmail = true;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

// Our own JWT issuance sits next to Identity, not inside it: Identity manages
// *who a user is* (storage, password checks, roles); the block below decides
// *how a logged-in user proves it* on every later request. AddIdentityCore does
// not include any token/JWT support out of the box.
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
                  ?? throw new InvalidOperationException("Jwt configuration section is missing.");
builder.Services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy => policy
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());
});

// Validates the bearer token on every incoming request against the same
// issuer/audience/signing key JwtTokenGenerator used to create it.
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
        };
    });

// Enables .RequireAuthorization() and role-based policies like
// .RequireAuthorization(policy => policy.RequireRole("Admin")) on endpoints.
builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // Applies any pending migrations, creating the database on first run.
    // Using real migrations (not EnsureCreated) is deliberate for this
    // template: it's the correct pattern for evolving a schema over time,
    // which EnsureCreated cannot do at all.
    await db.Database.MigrateAsync();

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    await DbSeeder.SeedAsync(roleManager, userManager);
}

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var feature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        var exception = feature?.Error;

        var (statusCode, title) = exception switch
        {
            ValidationException => (StatusCodes.Status400BadRequest, "Validation failed"),
            NotFoundException => (StatusCodes.Status404NotFound, "Resource not found"),
            UnauthorizedException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred")
        };

        context.Response.StatusCode = statusCode;

        var problem = exception switch
        {
            ValidationException validationException => Results.ValidationProblem(
                validationException.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())),
            _ => Results.Problem(title: title, statusCode: statusCode, detail: exception?.Message)
        };

        await problem.ExecuteAsync(context);
    });
});

// No HTTPS redirection: an http->https redirect to a different port is a cross-origin
// redirect, and browsers strip the Authorization header on those, breaking bearer auth
// for the static frontend. This API is plain HTTP by design for local development.
app.UseCors("Frontend");

// Order matters: authentication (who are you?) must run before authorization
// (are you allowed to do this?) — swapping these two lines breaks every
// RequireAuthorization() check in the app.
app.UseAuthentication();
app.UseAuthorization();

app.MapFeatureEndpoints(typeof(Program).Assembly);

app.Run();

public partial class Program;
