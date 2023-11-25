using FluentValidation;

namespace Identity.Clients.Requests;

public sealed record RemoveClientSecret : Command<Void>, IValid
{
    public static IValidator Validator { get; } = InlineValidator.For<RemoveClientSecret>(data =>
    {
    });
}
