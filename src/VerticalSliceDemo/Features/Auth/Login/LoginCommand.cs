using MediatR;

namespace VerticalSliceDemo.Features.Auth.Login;

public record LoginCommand(string Username, string Password) : IRequest<LoginResponse>;
