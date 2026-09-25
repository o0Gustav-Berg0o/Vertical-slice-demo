using FluentValidation;

namespace VerticalSliceTemplate.Features.Products.CreateProduct;

// A plain FluentValidation AbstractValidator<TRequest>. FastEndpoints finds
// and runs this automatically before HandleAsync (enabled via
// IncludeAbstractValidators in Program.cs) — the endpoint never needs to
// check these rules itself; a failed validation short-circuits with a 400
// before HandleAsync is ever called.
public class CreateProductValidator : AbstractValidator<CreateProductRequest>
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
