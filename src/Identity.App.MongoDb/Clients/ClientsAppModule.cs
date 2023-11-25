using Identity.Clients;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization;

namespace Identity.MongoDb.Clients;

public sealed class ClientsAppModule : IAppModule<ClientsAppModule>
{
    public string CollectionName { get; set; } = "client";

    public static void Add(IServiceCollection services, IConfiguration configuration, ClientsAppModule module)
    {
        BsonClassMap.RegisterClassMap<Client>(map =>
        {
            map.AutoMap();
            map.MapIdField(x => x.Id);
        });

        //services.AddSingleton<OpenIddictClientStore>();

        var builder = services
            .AddOpenIddict()
            .AddCore();

        // Note: Mongo uses simple binary comparison checks by default so the additional
        // query filtering applied by the default OpenIddict managers can be safely disabled.
        builder.DisableAdditionalFiltering();
        builder.SetDefaultApplicationEntity<Client>();

        // Note: the Mongo stores/resolvers don't depend on scoped/transient services and thus
        // can be safely registered as singleton services and shared/reused across requests.
        builder.ReplaceApplicationStoreResolver<OpenIddictClientStore>(ServiceLifetime.Singleton);
    }
}
