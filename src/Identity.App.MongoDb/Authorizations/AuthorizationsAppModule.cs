using Identity.Authorizations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization;

namespace Identity.MongoDb.Authorizations;

public sealed class AuthorizationsAppModule : IAppModule<AuthorizationsAppModule>
{
    public string AuthCollectionName { get; set; } = "authorization";
    
    public string TokenCollectionName { get; set; } = "token";

    public static void Add(IServiceCollection services, IConfiguration configuration, AuthorizationsAppModule module)
    {
        BsonClassMap.RegisterClassMap<Authorization>(map =>
        {
            map.AutoMap();
            //map.MapIdField(x => x.);
        });

        BsonClassMap.RegisterClassMap<Token>(map =>
        {
            map.AutoMap();
            //map.MapIdField(x => x.Name);
        });

        var builder = services
            .AddOpenIddict()
            .AddCore();

        // Note: Mongo uses simple binary comparison checks by default so the additional
        // query filtering applied by the default OpenIddict managers can be safely disabled.
        builder.DisableAdditionalFiltering();
        builder.SetDefaultAuthorizationEntity<Authorization>();
        builder.SetDefaultTokenEntity<Token>();

        // Note: the Mongo stores/resolvers don't depend on scoped/transient services and thus
        // can be safely registered as singleton services and shared/reused across requests.
        builder.ReplaceAuthorizationStoreResolver<OpenIddictAuthorizationStore>(ServiceLifetime.Singleton);
        builder.ReplaceTokenStoreResolver<OpenIddictTokenStore>(ServiceLifetime.Singleton);
    }
}
