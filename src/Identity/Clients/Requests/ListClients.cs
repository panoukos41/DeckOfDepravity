using FluentValidation;

namespace Identity.Clients.Requests;

public sealed record ListClients : Query<ResultSet<Client>>, IValid
{
    public static IValidator Validator { get; } = InlineValidator.For<ListClients>(data =>
    {
    });
}
