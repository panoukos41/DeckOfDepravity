namespace Identity.Clients.Requests;

public sealed record AddClient : Command<Client, Void>
{
    public AddClient(Client data) : base(data)
    {
    }
}
