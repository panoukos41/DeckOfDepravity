using Identity.Claims;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization;

namespace Identity.ClaimTypes;

public sealed class ClaimTypesAppModule : IAppModule<ClaimTypesAppModule>
{
    public string CollectionName { get; set; } = "claim-type";

    public static void Add(IServiceCollection services, IConfiguration configuration, ClaimTypesAppModule module)
    {
        BsonClassMap.RegisterClassMap<ClaimType>(map =>
        {
            map.AutoMap();
            map.MapIdField(x => x.Type);
        });
    }
}
