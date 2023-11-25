namespace Identity.ScopeTypes.Requests;

public sealed record AddScopeType : Command<ScopeType, Void>
{
    public AddScopeType(ScopeType data) : base(data)
    {
    }
}
