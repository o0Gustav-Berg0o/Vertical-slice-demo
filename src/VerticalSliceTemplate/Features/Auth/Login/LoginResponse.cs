namespace VerticalSliceTemplate.Features.Auth.Login;

public record LoginResponse(string Token, DateTime ExpiresAtUtc, string Username, IReadOnlyList<string> Roles);
