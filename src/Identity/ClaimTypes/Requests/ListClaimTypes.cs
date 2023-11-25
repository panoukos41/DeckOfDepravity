namespace Identity.Claims.Requests;

public sealed record ListClaimTypes : Query<ResultSet<ClaimType>>
{
    public string? Type { get; set; }

    public string? Name { get; set; }
}
