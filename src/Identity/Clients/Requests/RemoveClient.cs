using FluentValidation;

namespace Identity.Clients.Requests;

public sealed record RemoveClient : Command<Void>, IValid
{
    public string ClientId { get; }

    public RemoveClient(string clientId)
    {
        ClientId = clientId;
    }

    public static IValidator Validator { get; } = InlineValidator.For<RemoveClient>(data =>
    {

    });
}
