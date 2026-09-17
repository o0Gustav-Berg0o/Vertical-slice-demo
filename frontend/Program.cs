var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    WebRootPath = ".", // serve the frontend/ folder itself (vsa/, layered/, shared/) as static files
});

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.Run();
