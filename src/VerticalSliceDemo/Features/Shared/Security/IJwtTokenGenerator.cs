using VerticalSliceDemo.Data.Entities;

namespace VerticalSliceDemo.Features.Shared.Security;

public interface IJwtTokenGenerator
{
    (string Token, DateTime ExpiresAtUtc) GenerateToken(User user);
}
