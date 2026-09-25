using FluentValidation;
using MediatR;

namespace VerticalSliceTemplate.Features.Shared.Abstractions;

// A MediatR "pipeline behavior" wraps every request that goes through
// ISender.Send — this one runs before every handler in the app, so no
// individual Handler needs to call its own Validator manually. If a slice
// has a *Validator.cs implementing AbstractValidator<TCommand>, it's picked
// up here automatically (see ServiceCollectionExtensions.AddFeatures) and
// enforced before Handle() ever runs. A slice with no validator (e.g. a
// simple GetById query) just skips straight through.
public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        var failures = (await Task.WhenAll(
                validators.Select(v => v.ValidateAsync(context, cancellationToken))))
            .SelectMany(result => result.Errors)
            .Where(failure => failure is not null)
            .ToList();

        if (failures.Count > 0)
        {
            throw new ValidationException(failures);
        }

        return await next();
    }
}
