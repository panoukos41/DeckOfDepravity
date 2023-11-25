using FluentValidation;
using System.Text.Json.Nodes;

namespace Identity.Authorizations;

public sealed record Token : ITimeAudit, IValid
{
    /// <summary>
    /// Unique identifier associated with the current token.
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    /// The type of the current token.
    /// </summary>
    public required string Type { get; set; }

    /// <summary>
    /// The subject associated with the current token.
    /// </summary>
    public required string Subject { get; set; }

    /// <summary>
    /// The identifier of the client associated with the current token.
    /// </summary>
    public required string ClientId { get; set; }

    /// <summary>
    /// The identifier of the authorization associated with the current token.
    /// </summary>
    public required string AuthorizationId { get; set; }

    /// <summary>
    /// The status of the current token.
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// Audit containing the create and update times.
    /// </summary>
    public TimeAudit TimeAudit { get; init; } = TimeAudit.Empty;

    /// <summary>
    /// The redemption date of the current token.
    /// </summary>
    public DateTimeOffset? RedeemedAt { get; set; }

    /// <summary>
    /// The expiration date of the current token.
    /// </summary>
    public DateTimeOffset? ExpiresAt { get; set; }

    /// <summary>
    /// Additional properties associated with the current token.
    /// </summary>
    public JsonObject? Properties { get; set; }

    /// <summary>
    /// The payload of the current token, if applicable.
    /// Note: this property is only used for reference tokens
    /// and may be encrypted for security reasons.
    /// </summary>
    public string? Payload { get; set; }

    /// <summary>
    /// The reference identifier associated
    /// with the current token, if applicable.
    /// Note: this property is only used for reference tokens
    /// and may be hashed or encrypted for security reasons.
    /// </summary>
    public string? ReferenceId { get; set; }

    public static IValidator Validator { get; } = InlineValidator.For<Token>(data =>
    {
    });
}
