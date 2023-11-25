using FluentValidation;
using System.Text.Json.Nodes;

namespace Identity.Clients;

// todo: Add concurrency flag (to all models)

public sealed record Client : ITimeAudit, IValid
{
    public required string Id { get; set; }

    /// <summary>
    /// Client type: public or confidential.
    /// </summary>
    public required string Type { get; set; }

    /// <summary>
    /// Consent types: explicit, external, implicit or systematic.
    /// </summary>
    public string? ConsentType { get; set; }

    /// <summary>
    /// Localized display names with a default option.
    /// </summary>
    public LocalizedString Name { get; set; } = [];

    /// <summary>
    /// Localized display descriptions with a default option.
    /// </summary>
    public LocalizedString Description { get; set; } = [];

    /// <summary>
    /// The client secret.
    /// </summary>
    public string? Secret { get; set; }

    /// <summary>
    /// A list of flags that if available indicate a requirement must be met.
    /// </summary>
    public HashSet<string> Requirements { get; set; } = [];

    /// <summary>
    /// A list of grant_types and scopes.
    /// </summary>
    public HashSet<string> Permissions { get; set; } = [];

    /// <summary>
    /// Allowed cors URLs.
    /// </summary>
    public HashSet<Uri> CorsUris { get; set; } = [];

    /// <summary>
    /// Allowed redirect URLs.
    /// </summary>
    public HashSet<Uri> RedirectUris { get; set; } = [];

    /// <summary>
    /// Allowed post logout URLs.
    /// </summary>
    public HashSet<Uri> PostLogoutRedirectUris { get; set; } = [];

    /// <summary>
    /// Audit containing the create and update times.
    /// </summary>
    public TimeAudit TimeAudit { get; init; } = TimeAudit.Empty;

    /// <summary>
    /// Additional properties associated with the current client.
    /// </summary>
    public JsonObject? Properties { get; set; }

    public static IValidator Validator { get; } = InlineValidator.For<Client>(data =>
    {
        data.RuleFor(x => x.Id)
            .NotEmpty();

        data.RuleFor(x => x.Type)
            .Must(x => x is "public" or "confidential")
            .WithMessage("'{PropertyName}' must be 'public' or 'confidential'.");

        data.RuleFor(x => x.ConsentType)
            .Must(x => x is "explicit" or "external" or "implicit" or "systematic")
            .WithMessage("'{PropertyName}' must be 'explicit', 'external', 'implicit' or 'systematic'.");

        data.RuleFor(x => x.Name)
            .Must(x => x.Default is { Length: > 3 })
            .WithMessage("Name must at least have a default value greater than 3 characters.");

        // todo: More checks on name/description

        data.RuleForEach(x => x.CorsUris)
            .Must(x => x.IsAbsoluteUri && !x.IsFile && !x.IsUnc && x.Query.Length is 0)
            .WithMessage("Invalid CORS URL '{PropertyValue}'. It must be an absolute URL with no query params.");

        data.RuleForEach(x => x.RedirectUris)
            .Must(x => x.IsAbsoluteUri && !x.IsFile && !x.IsUnc && x.Query.Length is 0)
            .WithMessage("Invalid redirect URL '{PropertyValue}'. It must be an absolute URL with no query params.");

        data.RuleForEach(x => x.PostLogoutRedirectUris)
            .Must(x => x.IsAbsoluteUri && !x.IsFile && !x.IsUnc && x.Query.Length is 0)
            .WithMessage("Invalid post logout URL '{PropertyValue}'. It must be an absolute URL with no query params.");
    });
}
