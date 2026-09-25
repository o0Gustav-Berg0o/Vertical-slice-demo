using MediatR;

namespace VerticalSliceTemplate.Features.Auth.Login;

public record LoginCommand(string Username, string Password) : IRequest<LoginResponse>;
