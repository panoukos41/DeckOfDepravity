using Identity.Users;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization;

namespace Identity.MongoDb.Users;

public sealed class UsersAppModule : IAppModule<UsersAppModule>
{
    public string CollectionName { get; set; } = "user";

    public static void Add(IServiceCollection services, IConfiguration configuration, UsersAppModule module)
    {
        BsonClassMap.RegisterClassMap<User>(maps =>
        {
            maps.AutoMap();
            maps.MapIdField(x => x.Id);

            maps.MapProperty(x => x.Username)
                .SetIsRequired(true);

            maps.MapProperty(x => x.Email)
                .SetIsRequired(true);
        });
    }
}
