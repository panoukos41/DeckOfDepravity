namespace Identity.RoleTypes.Requests;

public sealed record AddRoleType : Command<RoleType, Void>
{
    public AddRoleType(RoleType data) : base(data)
    {
    }
}
