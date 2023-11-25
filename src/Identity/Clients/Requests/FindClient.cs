using FluentValidation;

namespace Identity.Clients.Requests;

public sealed record FindClient : Query<Client>, IValid
{
    public string ClientId { get; }

    public FindClient(string clientId)
    {
        ClientId = clientId;
    }

    public static IValidator Validator { get; } = InlineValidator.For<FindClient>(data =>
    {

    });
}
