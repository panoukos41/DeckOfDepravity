namespace Identity.RoleTypes.Requests;

public sealed record ListRoleTypes : Query<ResultSet<RoleType>>
{
}
