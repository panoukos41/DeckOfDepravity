using FluentValidation;

namespace Identity.RoleTypes;

public sealed record RoleType : ITimeAudit, IValid
{
    public required string Name { get; init; }

    public LocalizedString Description { get; init; } = [];

    /// <summary>
    /// Audit containing the create and update times.
    /// </summary>
    public TimeAudit TimeAudit { get; init; } = TimeAudit.Empty;

    public static IValidator Validator { get; } = InlineValidator.For<RoleType>(data =>
    {
        // todo: Implement
    });
}
