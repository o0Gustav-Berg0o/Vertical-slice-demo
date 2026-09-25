using MediatR;

namespace VerticalSliceTemplate.Features.Auth.Register;

public record RegisterCommand(string Username, string Email, string Password) : IRequest<RegisterResponse>;
