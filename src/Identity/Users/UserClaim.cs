using System.Diagnostics.CodeAnalysis;

namespace Identity.Users;

public sealed record UserClaim
{
    public required string Type { get; init; }

    public required string Value { get; init; }

    public UserClaim()
    {
    }

    [SetsRequiredMembers]
    public UserClaim(string type, string value)
    {
        Type = type;
        Value = value;
    }
}
