namespace Identity.Claims.Requests;

public sealed record AddClaimType : Command<ClaimType, Void>
{
    public AddClaimType(ClaimType data) : base(data)
    {
    }
}
