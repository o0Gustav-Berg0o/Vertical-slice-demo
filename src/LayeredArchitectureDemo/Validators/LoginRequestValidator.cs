using FluentValidation;
using LayeredArchitectureDemo.Models.Dtos;

namespace LayeredArchitectureDemo.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Username).NotEmpty();
        RuleFor(x => x.Password).NotEmpty();
    }
}
