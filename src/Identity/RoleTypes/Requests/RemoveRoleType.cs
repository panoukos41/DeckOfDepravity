using FluentValidation;

namespace Identity.RoleTypes.Requests;

public sealed record RemoveRoleType : Command<Void>, IValid
{
    public required string Name { get; init; }

    public static IValidator Validator { get; } = InlineValidator.For<RemoveRoleType>(data =>
    {
        data.RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(50);
    });
}
