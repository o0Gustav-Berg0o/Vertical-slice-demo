namespace VerticalSliceDemo.Features.Shared.Abstractions;

public class NotFoundException(string message) : Exception(message);
