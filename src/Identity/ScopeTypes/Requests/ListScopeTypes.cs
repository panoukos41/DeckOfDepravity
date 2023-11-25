namespace Identity.ScopeTypes.Requests;

public sealed record ListScopeTypes : Query<ResultSet<ScopeType>>
{
}
