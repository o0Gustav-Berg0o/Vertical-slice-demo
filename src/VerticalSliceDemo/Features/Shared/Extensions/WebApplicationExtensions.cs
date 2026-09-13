using System.Reflection;
using VerticalSliceDemo.Features.Shared.Abstractions;

namespace VerticalSliceDemo.Features.Shared.Extensions;

public static class WebApplicationExtensions
{
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
