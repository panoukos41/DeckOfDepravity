using Core.Commons;
using Identity.Users;
using System.Security.Claims;

namespace Identity.Users.Requests;

public sealed record ListUsers : Query<ResultSet<User>>
{
    public string? Username { get; set; }

    public string? Email { get; init; }

    public Phone? Phone { get; set; }

    public HashSet<Address>? Addresses { get; set; }

    public HashSet<Claim>? Claims { get; set; }
}
