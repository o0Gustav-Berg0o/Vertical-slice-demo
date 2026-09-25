using FluentValidation;

namespace VerticalSliceTemplate.Features.Auth.Register;

public class RegisterValidator : AbstractValidator<RegisterCommand>
{
    public RegisterValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        // Mirrors the password policy configured on AddIdentityCore in Program.cs.
        // This isn't duplication for its own sake: FluentValidation runs first and
        // gives a clean, field-level 400 before Identity is even touched; Identity's
        // own PasswordOptions check is what actually gets enforced and is the real
        // last line of defense (e.g. for any future code path that calls
        // UserManager.CreateAsync directly, bypassing this validator).
        RuleFor(x => x.Password)
            .MinimumLength(8)
            .Matches("[0-9]").WithMessage("Password must contain at least one digit.")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.");
    }
}
