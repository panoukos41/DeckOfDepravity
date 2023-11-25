using Core.Commons;
using FluentValidation;
using Identity.Users;

namespace Identity.Users.Requests;

public sealed record PatchUser : Command<Void>
{
    public required Uuid Id { get; init; }

    public string? Username { get; set; }

    public string? Email { get; init; }

    public Phone? Phone { get; set; }

    //public HashSet<Address>? Addresses { get; set; } = [];

    //public HashSet<Claim> Claims { get; set; } = [];

    public static IValidator Validator { get; } = InlineValidator.For<User>(data =>
    {
    });
}
