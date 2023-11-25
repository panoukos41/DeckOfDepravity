using FluentValidation;
using System.Text.Json.Nodes;

namespace Identity.Authorizations;

public sealed record Authorization : ITimeAudit, IValid
{
    /// <summary>
    /// The unique identifier associated with the current authorization.
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    /// The type of the current authorization.
    /// </summary>
    public required string Type { get; set; }

    /// <summary>
    /// The subject associated with the current authorization.
    /// </summary>
    public required string Subject { get; set; }

    /// <summary>
    /// The identifier of the client associated with the current authorization.
    /// </summary>
    public required string ClientId { get; set; }

    /// <summary>
    /// The status of the current authorization.
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// Audit containing the create and update times.
    /// </summary>
    public TimeAudit TimeAudit { get; init; } = TimeAudit.Empty;

    /// <summary>
    /// The scopes associated with the current authorization.
    /// </summary>
    public HashSet<string> Scopes { get; set; } = new(StringComparer.Ordinal);

    /// <summary>
    /// Additional properties associated with the current authorization.
    /// </summary>
    public JsonObject? Properties { get; set; }

    public static IValidator Validator { get; } = InlineValidator.For<Authorization>(data =>
    {
        // todo: Implement validation.
    });
}
