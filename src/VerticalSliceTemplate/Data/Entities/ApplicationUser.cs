using Microsoft.AspNetCore.Identity;

namespace VerticalSliceTemplate.Data.Entities;

// Extends ASP.NET Core Identity's built-in IdentityUser instead of a hand-rolled
// user entity. This gives us, for free and battle-tested: secure password
// hashing (PasswordHasher<TUser>, used internally by UserManager), account
// lockout after repeated failed logins, and role/claim storage.
//
// We deliberately keep IdentityUser's default string (GUID) primary key rather
// than switching to IdentityUser<int>. Neither Product nor Order has a foreign
// key to a user in this template, so there is no real payoff for fighting
// Identity's default key type — doing so would mean using the more complex
// IdentityDbContext<TUser, TRole, TKey> overload for no benefit. If your own
// app needs a User -> Order relationship, referencing this string Id is the
// idiomatic way to do it.
public class ApplicationUser : IdentityUser;
