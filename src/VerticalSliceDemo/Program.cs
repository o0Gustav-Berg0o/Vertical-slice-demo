using FluentValidation;
using Microsoft.EntityFrameworkCore;
using VerticalSliceDemo.Data;
using VerticalSliceDemo.Features.Shared.Abstractions;
using VerticalSliceDemo.Features.Shared.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? "Data Source=verticalslicedemo.db"));

builder.Services.AddFeatures(typeof(Program).Assembly);

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
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

app.UseHttpsRedirection();

app.MapFeatureEndpoints(typeof(Program).Assembly);

app.Run();

public partial class Program;
