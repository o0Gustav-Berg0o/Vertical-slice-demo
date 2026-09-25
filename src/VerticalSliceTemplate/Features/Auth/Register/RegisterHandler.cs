using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Identity;
using VerticalSliceTemplate.Data.Entities;

namespace VerticalSliceTemplate.Features.Auth.Register;

public class RegisterHandler(UserManager<ApplicationUser> userManager) : IRequestHandler<RegisterCommand, RegisterResponse>
{
    public async Task<RegisterResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var user = new ApplicationUser { UserName = request.Username, Email = request.Email };
        var result = await userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            // Translate Identity's own result type into FluentValidation's, so the
            // existing global exception handler (which only knows about
            // FluentValidation's ValidationException) can turn this into the same
            // kind of 400 response as every other validation failure in the app,
            // with no extra code in Program.cs.
            var failures = result.Errors.Select(e => new ValidationFailure(nameof(request.Password), e.Description));
            throw new ValidationException(failures);
        }

        // New users get no role by default (least privilege) — only the seeded
        // demo admin starts with the Admin role. Promoting a user is something
        // an existing Admin would do, which this template doesn't implement but
        // would be a natural next feature slice to add.
        return new RegisterResponse(user.UserName!, user.Email!);
    }
}
