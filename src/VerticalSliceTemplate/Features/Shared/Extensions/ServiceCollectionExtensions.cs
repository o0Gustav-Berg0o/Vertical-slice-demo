using System.Reflection;
using FluentValidation;
using VerticalSliceTemplate.Features.Shared.Abstractions;

namespace VerticalSliceTemplate.Features.Shared.Extensions;

public static class ServiceCollectionExtensions
{
    // The "no manual DI registration" part of the vertical slice convention:
    // this single call scans the whole assembly and wires up every feature at
    // once, so adding a new slice never means touching Program.cs.
    public static IServiceCollection AddFeatures(this IServiceCollection services, Assembly assembly)
    {
        // Finds every class implementing IRequestHandler<,> and registers it —
        // this is how MediatR knows which Handler to call for a given
        // Command/Query without you registering each one by hand.
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));

        // Finds every class implementing AbstractValidator<T> (every *Validator.cs)
        // and registers it as IValidator<T>, which ValidationBehavior below then
        // resolves per-request.
        services.AddValidatorsFromAssembly(assembly);

        // Plugs ValidationBehavior into MediatR's pipeline for every
        // request/response pair, so validation runs before any handler does.
        services.AddTransient(typeof(MediatR.IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        return services;
    }
}
