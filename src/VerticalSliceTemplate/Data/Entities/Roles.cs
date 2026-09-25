namespace VerticalSliceTemplate.Data.Entities;

// Role names as constants instead of magic strings, so a typo in
// RequireRole("Admn") is a compile error instead of a silent 403 at runtime.
public static class Roles
{
    public const string Admin = "Admin";
    public const string User = "User";
}
