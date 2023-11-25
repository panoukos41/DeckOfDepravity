namespace Identity.Users.Requests;

public sealed record FindUser : FindQuery<User>
{
    public FindUser(Uuid id) : base(id)
    {
    }
}
