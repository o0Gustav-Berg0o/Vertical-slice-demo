using System.Reflection;
using FluentValidation;
using VerticalSliceDemo.Features.Shared.Abstractions;

namespace VerticalSliceDemo.Features.Shared.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFeatures(this IServiceCollection services, Assembly assembly)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        services.AddValidatorsFromAssembly(assembly);
        services.AddTransient(typeof(MediatR.IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        return services;
    }
}
