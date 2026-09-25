namespace VerticalSliceTemplate.Features.Shared.Abstractions;

// Every feature's *Endpoint class implements this. The "static abstract" member
// (a C# 11+ feature) lets WebApplicationExtensions.MapFeatureEndpoints find every
// implementing type via reflection and call MapEndpoint on each one WITHOUT
// needing an instance of the class first — endpoint classes never get
// constructed, they're just a place to hang a route-mapping method that gets
// auto-discovered. That's the whole trick behind "no manual registration in
// Program.cs": implement this interface in the right namespace, and you're done.
public interface IEndpoint
{
    static abstract void MapEndpoint(IEndpointRouteBuilder app);
}
