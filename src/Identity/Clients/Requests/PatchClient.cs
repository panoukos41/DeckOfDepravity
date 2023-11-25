using FluentValidation;

namespace Identity.Clients.Requests;

public sealed record PatchClient : Command<Void>, IValid
{
    public static IValidator Validator { get; } = InlineValidator.For<PatchClient>(data =>
    {

    });
}
