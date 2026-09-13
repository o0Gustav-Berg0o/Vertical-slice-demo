using LayeredArchitectureDemo.Models.Entities;

namespace LayeredArchitectureDemo.Common;

public interface IJwtTokenGenerator
{
    (string Token, DateTime ExpiresAtUtc) GenerateToken(User user);
}
