using Identity.RoleTypes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization;

namespace Identity.MongoDb.RoleTypes;

public sealed class RoleTypesAppModule : IAppModule<RoleTypesAppModule>
{
    public string CollectionName { get; set; } = "roles";

    public static void Add(IServiceCollection services, IConfiguration configuration, RoleTypesAppModule module)
    {
        BsonClassMap.RegisterClassMap<RoleType>(map =>
        {
            map.AutoMap();
            map.MapIdField(x => x.Name);
        });
    }
}
