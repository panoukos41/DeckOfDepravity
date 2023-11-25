using FluentValidation;
using System.Text.Json.Nodes;

namespace Identity.Users;

public sealed record User : IEntity, ITimeAudit, IValid
{
    // todo: Consider breaking this in two parts to avoid Id being required in json.
    public required Uuid Id { get; init; }

    public required string Username { get; set; }

    public string Email { get; init; } = string.Empty;

    public Phone Phone { get; set; } = Phone.Empty;

    public HashSet<Address> Addresses { get; set; } = [];

    public HashSet<UserClaim> Claims { get; set; } = [];

    /// <summary>
    /// Audit containing the create and update times.
    /// </summary>
    public TimeAudit TimeAudit { get; init; } = TimeAudit.Empty;

    /// <summary>
    /// A list of flags. Flags ignore case.
    /// </summary>
    public HashSet<string> Flags { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    // todo: Consider using byte[]
    public HashSet<string> Passwords { get; set; } = new(StringComparer.Ordinal);

    /// <summary>
    /// Additional properties associated with the current user.
    /// </summary>
    public JsonObject? Properties { get; init; }

    public static IValidator Validator { get; } = InlineValidator.For<User>(data =>
    {
    });
}
