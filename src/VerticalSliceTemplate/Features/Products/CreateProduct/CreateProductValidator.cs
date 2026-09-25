using FluentValidation;

namespace VerticalSliceTemplate.Features.Products.CreateProduct;

// Optional, but present in most slices: an AbstractValidator<TCommand> is
// found automatically by AddFeatures and run automatically by
// ValidationBehavior before Handle() below ever executes. The Handler never
// needs to check these rules itself — by the time it runs, the command is
// already known-valid.
public class CreateProductValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .MaximumLength(2000);

        RuleFor(x => x.Price)
            .GreaterThan(0);

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0);
    }
}
