using FluentValidation;
using Identity.ClaimTypes;
using Identity.MongoDb.Authorizations;
using Identity.MongoDb.Clients;
using Identity.MongoDb.RoleTypes;
using Identity.MongoDb.ScopeTypes;
using Identity.MongoDb.Users;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MongoDB.Driver;

namespace Identity.MongoDb;

public sealed class IdentityAppModule : IAppModule<IdentityAppModule>, IValid
{
    public string DatabaseName { get; set; } = string.Empty;

    public string ConnectionString { get; set; } = string.Empty;

    public static void Add(IServiceCollection services, IConfiguration configuration, IdentityAppModule module)
    {
        module.ValidateAndThrow();

        services.AddSingleton(static sp =>
        {
            var module = sp.GetRequiredService<IdentityAppModule>();
            var client = new MongoClient(module.ConnectionString);
            return new IdentityMongoDb(client, module.DatabaseName);
        });

        services.TryAddSingleton(TimeProvider.System);

        services.AddAppModule<AuthorizationsAppModule>(configuration);
        services.AddAppModule<ClaimTypesAppModule>(configuration);
        services.AddAppModule<ClientsAppModule>(configuration);
        services.AddAppModule<RoleTypesAppModule>(configuration);
        services.AddAppModule<ScopeTypesAppModule>(configuration);
        services.AddAppModule<UsersAppModule>(configuration);

        var builder = services
            .AddOpenIddict()
            .AddCore();

        // Note: Mongo uses simple binary comparison checks by default so the additional
        // query filtering applied by the default OpenIddict managers can be safely disabled.
        builder.DisableAdditionalFiltering();

        services.AddAppModule<CoreMongoDbAppModule>(configuration);
    }

    public static IValidator Validator { get; } = InlineValidator.For<IdentityAppModule>(data =>
    {
        data.RuleFor(x => x.DatabaseName)
            .NotEmpty()
            .Must(x => !x.Contains('.')).WithMessage("'{PropertyName}' must not contain the '.' character.");

        data.RuleFor(x => x.ConnectionString)
            .NotEmpty();
    });
}
