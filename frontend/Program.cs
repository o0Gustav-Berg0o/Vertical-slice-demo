var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    WebRootPath = ".", // serve the frontend/ folder itself (index.html, shared/) as static files
});

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.Run();
