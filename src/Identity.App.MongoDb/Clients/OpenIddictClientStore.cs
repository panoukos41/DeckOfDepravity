using Core.MongoDb.Commons;
using Identity.Clients;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using OpenIddict.Abstractions;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using static OpenIddict.Abstractions.OpenIddictExceptions;
using SR = OpenIddict.Abstractions.OpenIddictResources;

namespace Identity.MongoDb.Clients;

public sealed class OpenIddictClientStore :
    IOpenIddictApplicationStoreResolver,
    IOpenIddictApplicationStore<Client>
{
    private readonly IMongoCollection<Client> clients;
    private readonly TimeProvider timeProvider;

    public OpenIddictClientStore(ClientsAppModule module, IdentityMongoDb mongoDb, TimeProvider timeProvider)
    {
        clients = mongoDb.GetCollection<Client>(module.CollectionName);
        this.timeProvider = timeProvider;
    }

    public IOpenIddictApplicationStore<TApplication> Get<TApplication>() where TApplication : class
    {
        return (this as IOpenIddictApplicationStore<TApplication>)!;
    }

    public ValueTask<long> CountAsync(CancellationToken cancellationToken)
    {
        return new(clients.EstimatedDocumentCountAsync(null, cancellationToken));
    }

    public ValueTask<long> CountAsync<TResult>(Func<IQueryable<Client>, IQueryable<TResult>> query, CancellationToken cancellationToken)
    {
        var task = clients
            .AsQueryable()
            .ApplyQuery(query)
            .LongCountAsync(cancellationToken);
        return new(task);
    }

    public IAsyncEnumerable<Client> ListAsync(int? count, int? offset, CancellationToken cancellationToken)
    {
        var query = clients.AsQueryable().ApplyQuery(query => query.OrderBy(x => x.Id));
        if (offset is { } offsetValue)
        {
            query = query.Skip(offsetValue);
        }
        if (count is { } countValue)
        {
            query = query.Take(countValue);
        }
        return query.ToAsyncEnumerable(cancellationToken);
    }

    public IAsyncEnumerable<TResult> ListAsync<TState, TResult>(Func<IQueryable<Client>, TState, IQueryable<TResult>> query, TState state, CancellationToken cancellationToken)
    {
        return clients
            .AsQueryable()
            .ApplyQuery(state, query)
            .ToAsyncEnumerable(cancellationToken);
    }

    public ValueTask<TResult?> GetAsync<TState, TResult>(Func<IQueryable<Client>, TState, IQueryable<TResult>> query, TState state, CancellationToken cancellationToken)
    {
        var task = clients
            .AsQueryable()
            .ApplyQuery(state, query)
            .FirstOrDefaultAsync(cancellationToken);
        return new(task!);
    }

    public ValueTask<Client?> FindByClientIdAsync(string identifier, CancellationToken cancellationToken)
    {
        var filter = Builders<Client>.Filter.Eq(x => x.Id, identifier);
        return new(clients.Find(filter).FirstOrDefaultAsync(cancellationToken)!);
    }

    public ValueTask<Client?> FindByIdAsync(string identifier, CancellationToken cancellationToken)
    {
        var filter = Builders<Client>.Filter.Eq(x => x.Id, identifier);
        return new(clients.Find(filter).FirstOrDefaultAsync(cancellationToken)!);
    }

    public IAsyncEnumerable<Client> FindByRedirectUriAsync([StringSyntax("Uri")] string uri, CancellationToken cancellationToken)
    {
        var filter = Builders<Client>.Filter.AnyEq(x => x.RedirectUris, new Uri(uri));
        return clients
            .FindAsync(filter, null, cancellationToken)
            .ToAsyncEnumerable(cancellationToken);
    }

    public IAsyncEnumerable<Client> FindByPostLogoutRedirectUriAsync([StringSyntax("Uri")] string uri, CancellationToken cancellationToken)
    {
        var filter = Builders<Client>.Filter.AnyEq(x => x.PostLogoutRedirectUris, new Uri(uri));
        return clients
            .FindAsync(filter, null, cancellationToken)
            .ToAsyncEnumerable(cancellationToken);
    }

    #region CRUD

    public ValueTask<Client> InstantiateAsync(CancellationToken cancellationToken)
    {
        return new(new Client
        {
            Id = string.Empty,
            Type = string.Empty,
            TimeAudit = new TimeAudit(timeProvider)
        });
    }

    public ValueTask CreateAsync(Client client, CancellationToken cancellationToken)
    {
        if (client.Validate().Errors is { Count: > 0 } errors)
        {
            throw new ProblemException(Problems.Validation.WithValidationErrors(errors));
        }
        return new(clients.InsertOneAsync(client, null, cancellationToken));
    }

    public async ValueTask UpdateAsync(Client client, CancellationToken cancellationToken)
    {
        if (client.Validate().Errors is { Count: > 0 } errors)
        {
            throw new ProblemException(Problems.Validation.WithValidationErrors(errors));
        }

        client.UpdatedAt(timeProvider);

        var filter = Builders<Client>.Filter.Eq(x => x.Id, client.Id);
        var result = await clients.ReplaceOneAsync(filter, client, cancellationToken: cancellationToken);

        if (result.MatchedCount is 0)
        {
            throw new ConcurrencyException(SR.GetResourceString(SR.ID0245));
        }
    }

    public ValueTask DeleteAsync(Client client, CancellationToken cancellationToken)
    {
        var filter = Builders<Client>.Filter.Eq(x => x.Id, client.Id);
        return new(clients.DeleteOneAsync(filter, cancellationToken));
    }

    #endregion

    #region Get Properties

    public ValueTask<string?> GetIdAsync(Client client, CancellationToken cancellationToken)
    {
        return new(client.Id);
    }

    public ValueTask<string?> GetClientIdAsync(Client client, CancellationToken cancellationToken)
    {
        return new(client.Id);
    }

    public ValueTask<string?> GetClientTypeAsync(Client client, CancellationToken cancellationToken)
    {
        return new(client.Type);
    }

    public ValueTask<string?> GetConsentTypeAsync(Client client, CancellationToken cancellationToken)
    {
        return new(client.ConsentType);
    }

    public ValueTask<string?> GetDisplayNameAsync(Client client, CancellationToken cancellationToken)
    {
        return new(client.Name.Default);
    }

    public ValueTask<ImmutableDictionary<CultureInfo, string>> GetDisplayNamesAsync(Client client, CancellationToken cancellationToken)
    {
        return new(client.Name.ToImmutableDictionary(
            pair => CultureInfo.GetCultureInfo(pair.Key),
            pair => pair.Value
        ));
    }

    public ValueTask<string?> GetClientSecretAsync(Client client, CancellationToken cancellationToken)
    {
        return new(client.Secret);
    }

    public ValueTask<ImmutableArray<string>> GetRequirementsAsync(Client client, CancellationToken cancellationToken)
    {
        return new([.. client.Requirements]);
    }

    public ValueTask<ImmutableArray<string>> GetPermissionsAsync(Client client, CancellationToken cancellationToken)
    {
        return new([.. client.Permissions]);
    }

    public ValueTask<ImmutableArray<string>> GetRedirectUrisAsync(Client client, CancellationToken cancellationToken)
    {
        return new([.. client.RedirectUris.Select(x => x.ToString())]);
    }

    public ValueTask<ImmutableArray<string>> GetPostLogoutRedirectUrisAsync(Client client, CancellationToken cancellationToken)
    {
        return new([.. client.PostLogoutRedirectUris.Select(x => x.ToString())]);
    }

    public ValueTask<ImmutableDictionary<string, JsonElement>> GetPropertiesAsync(Client client, CancellationToken cancellationToken)
    {
        var props = client.Properties.ToImmutableDictionary();
        return new(props ?? ImmutableDictionary.Create<string, JsonElement>());
    }

    #endregion

    #region Set Properties

    public ValueTask SetClientIdAsync(Client client, string? identifier, CancellationToken cancellationToken)
    {
        client.Id = identifier ?? string.Empty;
        return new();
    }

    public ValueTask SetClientTypeAsync(Client client, string? type, CancellationToken cancellationToken)
    {
        client.Type = type ?? string.Empty;
        return new();
    }

    public ValueTask SetConsentTypeAsync(Client client, string? type, CancellationToken cancellationToken)
    {
        client.ConsentType = type ?? string.Empty;
        return new();
    }

    public ValueTask SetDisplayNameAsync(Client client, string? name, CancellationToken cancellationToken)
    {
        client.Name.Default = name;
        return new();
    }

    public ValueTask SetDisplayNamesAsync(Client client, ImmutableDictionary<CultureInfo, string> names, CancellationToken cancellationToken)
    {
        var defaultName = client.Name.Default;
        client.Name.Clear();
        client.Name.Default = defaultName;
        foreach (var (culture, value) in names)
        {
            client.Name[culture.Name] = value;
        }
        return new();
    }

    public ValueTask SetRequirementsAsync(Client client, ImmutableArray<string> requirements, CancellationToken cancellationToken)
    {
        client.Requirements.Clear();
        foreach (var item in requirements)
        {
            client.Requirements.Add(item);
        }
        return new();
    }

    public ValueTask SetPermissionsAsync(Client client, ImmutableArray<string> permissions, CancellationToken cancellationToken)
    {
        client.Permissions.Clear();
        foreach (var item in permissions)
        {
            client.Permissions.Add(item);
        }
        return new();
    }

    public ValueTask SetClientSecretAsync(Client client, string? secret, CancellationToken cancellationToken)
    {
        client.Secret = secret;
        return new();
    }

    public ValueTask SetRedirectUrisAsync(Client client, ImmutableArray<string> uris, CancellationToken cancellationToken)
    {
        client.RedirectUris.Clear();
        foreach (var item in uris)
        {
            client.RedirectUris.Add(new Uri(item));
        }
        return new();
    }

    public ValueTask SetPostLogoutRedirectUrisAsync(Client client, ImmutableArray<string> uris, CancellationToken cancellationToken)
    {
        client.PostLogoutRedirectUris.Clear();
        foreach (var item in uris)
        {
            client.PostLogoutRedirectUris.Add(new Uri(item));
        }
        return new();
    }

    public ValueTask SetPropertiesAsync(Client client, ImmutableDictionary<string, JsonElement> properties, CancellationToken cancellationToken)
    {
        client.Properties = new JsonObject().CopyFrom(properties);
        return new();
    }

    #endregion
}
