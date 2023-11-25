namespace Identity.Users.Requests;

public sealed record AddUser : Command<User, CreatedResponse>
{
    public AddUser(User data) : base(data)
    {
    }
}
