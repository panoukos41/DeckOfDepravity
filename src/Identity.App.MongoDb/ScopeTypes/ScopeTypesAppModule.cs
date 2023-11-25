using Identity.ScopeTypes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization;

namespace Identity.MongoDb.ScopeTypes;

public sealed class ScopeTypesAppModule : IAppModule<ScopeTypesAppModule>
{
    public string CollectionName { get; set; } = "scope";

    public static void Add(IServiceCollection services, IConfiguration configuration, ScopeTypesAppModule module)
    {
        BsonClassMap.RegisterClassMap<ScopeType>(map =>
        {
            map.AutoMap();
            map.MapIdField(x => x.Id);
        });

        //services.AddSingleton<OpenIddictScopeTypeStore>();

        var builder = services
            .AddOpenIddict()
            .AddCore();

        // Note: Mongo uses simple binary comparison checks by default so the additional
        // query filtering applied by the default OpenIddict managers can be safely disabled.
        builder.DisableAdditionalFiltering();
        builder.SetDefaultScopeEntity<ScopeType>();

        // Note: the Mongo stores/resolvers don't depend on scoped/transient services and thus
        // can be safely registered as singleton services and shared/reused across requests.
        builder.ReplaceScopeStoreResolver<OpenIddictScopeTypeStore>(ServiceLifetime.Singleton);
    }
}
