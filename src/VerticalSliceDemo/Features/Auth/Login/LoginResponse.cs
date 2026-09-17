namespace VerticalSliceDemo.Features.Auth.Login;

public record LoginResponse(string Token, DateTime ExpiresAtUtc, string Username, string Role);
