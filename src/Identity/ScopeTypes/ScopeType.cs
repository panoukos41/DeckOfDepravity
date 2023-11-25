using FluentValidation;
using System.Text.Json.Nodes;

namespace Identity.ScopeTypes;

public sealed record ScopeType : ITimeAudit, IValid
{
    public required string Id { get; set; }

    public LocalizedString Name { get; set; } = [];

    public LocalizedString Description { get; set; } = [];

    public HashSet<string> Claims { get; set; } = [];

    /// <summary>
    /// Audit containing the create and update times.
    /// </summary>
    public TimeAudit TimeAudit { get; init; } = TimeAudit.Empty;

    /// <summary>
    /// Additional properties associated with the current authorization.
    /// </summary>
    public JsonObject? Properties { get; set; }

    public static IValidator Validator { get; } = InlineValidator.For<ScopeType>(data =>
    {
        data.RuleFor(x => x.Id)
            .NotEmpty();

        // todo: More checks on name/description
        //data.RuleFor(x => x.Name)
        //    .NotEmpty()
        //    .Length(3, 50)
        //    .When(x => x.Name is not null);

        //data.RuleFor(x => x.Description)
        //    .NotEmpty()
        //    .Length(1, 250)
        //    .When(x => x.Description is not null);
    });
}
