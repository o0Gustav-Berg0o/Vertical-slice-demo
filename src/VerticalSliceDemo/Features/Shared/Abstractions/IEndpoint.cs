namespace VerticalSliceDemo.Features.Shared.Abstractions;

public interface IEndpoint
{
    static abstract void MapEndpoint(IEndpointRouteBuilder app);
}
