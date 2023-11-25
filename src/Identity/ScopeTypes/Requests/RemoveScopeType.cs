using FluentValidation;

namespace Identity.ScopeTypes.Requests;

public sealed record RemoveScopeType : Command<Void>, IValid
{
    public string ScopeId { get; }

    public RemoveScopeType(string scopeId)
    {
        ScopeId = scopeId;
    }

    public static IValidator Validator { get; } = InlineValidator.For<RemoveScopeType>(data =>
    {
        data.RuleFor(x => x.ScopeId)
            .NotEmpty();
    });
}
