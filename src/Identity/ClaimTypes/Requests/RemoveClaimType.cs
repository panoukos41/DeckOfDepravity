namespace Identity.Claims.Requests;

public sealed record RemoveClaimType : Command<Void>
{
    public string Type { get; }

    public RemoveClaimType(string type)
    {
        Type = type;
    }
}
