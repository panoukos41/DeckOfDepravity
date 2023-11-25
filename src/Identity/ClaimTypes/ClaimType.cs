using FluentValidation;

namespace Identity.Claims;

public sealed record ClaimType : ITimeAudit, IValid
{
    public required string Type { get; init; }

    // todo: Refactor to localized string.

    public string? Name { get; init; }

    public string? Description { get; init; }

    public HashSet<string> Suggestions { get; init; } = [];

    /// <summary>
    /// Audit containing the create and update times.
    /// </summary>
    public TimeAudit TimeAudit { get; init; } = TimeAudit.Empty;

    public static IValidator Validator { get; } = InlineValidator.For<ClaimType>(data =>
    {
        data.RuleFor(x => x.Type)
            .NotEmpty()
            .Length(3, 50);

        data.RuleFor(x => x.Name)
            .Length(3, 50)
            .When(x => x.Name is { });

        data.RuleFor(x => x.Description)
            .Length(3, 250)
            .When(x => x.Description is { });

        data.RuleForEach(x => x.Suggestions)
            .NotEmpty()
            .Length(1, 50);
    });
}
