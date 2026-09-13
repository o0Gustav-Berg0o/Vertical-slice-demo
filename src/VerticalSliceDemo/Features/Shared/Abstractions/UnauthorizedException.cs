namespace VerticalSliceDemo.Features.Shared.Abstractions;

public class UnauthorizedException(string message) : Exception(message);
