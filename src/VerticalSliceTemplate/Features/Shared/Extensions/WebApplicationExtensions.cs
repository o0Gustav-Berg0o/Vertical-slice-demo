using System.Reflection;
using VerticalSliceTemplate.Features.Shared.Abstractions;

namespace VerticalSliceTemplate.Features.Shared.Extensions;

public static class WebApplicationExtensions
{
    // Called once from Program.cs. Finds every non-abstract class in the
    // assembly implementing IEndpoint, then calls its static MapEndpoint(app)
    // — this is the reflection counterpart to IEndpoint's "static abstract"
    // member. The practical effect: create an *Endpoint.cs file implementing
    // IEndpoint anywhere under Features/, and its route is registered
    // automatically the next time the app starts. No list of endpoints to
    // maintain, nothing to add here.
    public static IEndpointRouteBuilder MapFeatureEndpoints(this IEndpointRouteBuilder app, Assembly assembly)
    {
        var endpointTypes = assembly.GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false } &&
                        t.GetInterfaces().Contains(typeof(IEndpoint)));

        foreach (var type in endpointTypes)
        {
            var method = type.GetMethod(nameof(IEndpoint.MapEndpoint), BindingFlags.Public | BindingFlags.Static);
            method?.Invoke(null, [app]);
        }

        return app;
    }
}
