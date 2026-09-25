using VerticalSliceTemplate.Data.Entities;

namespace VerticalSliceTemplate.Features.Shared.Security;

public interface IJwtTokenGenerator
{
    // Roles are passed in explicitly (rather than looked up here) because
    // fetching them requires UserManager, which already ran in the calling
    // handler — no reason to hit the database twice for the same user.
    (string Token, DateTime ExpiresAtUtc) GenerateToken(ApplicationUser user, IEnumerable<string> roles);
}
