using Core.MongoDb.Commons;
using Identity.ScopeTypes;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using OpenIddict.Abstractions;
using System.Collections.Immutable;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using static OpenIddict.Abstractions.OpenIddictExceptions;
using SR = OpenIddict.Abstractions.OpenIddictResources;

namespace Identity.MongoDb.ScopeTypes;

public sealed class OpenIddictScopeTypeStore :
    IOpenIddictScopeStoreResolver,
    IOpenIddictScopeStore<ScopeType>
{

    private readonly IMongoCollection<ScopeType> scopes;
    private readonly TimeProvider timeProvider;

    public OpenIddictScopeTypeStore(ScopeTypesAppModule module, IdentityMongoDb mongoDb, TimeProvider timeProvider)
    {
        scopes = mongoDb.GetCollection<ScopeType>(module.CollectionName);
        this.timeProvider = timeProvider;
    }

    public IOpenIddictScopeStore<TScope> Get<TScope>() where TScope : class
    {
        return (this as IOpenIddictScopeStore<TScope>)!;
    }

    public ValueTask<long> CountAsync(CancellationToken cancellationToken)
    {
        return new(scopes.EstimatedDocumentCountAsync(null, cancellationToken));
    }

    public ValueTask<long> CountAsync<TResult>(Func<IQueryable<ScopeType>, IQueryable<TResult>> query, CancellationToken cancellationToken)
    {
        var task = scopes
            .AsQueryable()
            .ApplyQuery(query)
            .LongCountAsync(cancellationToken);
        return new(task);
    }

    public IAsyncEnumerable<ScopeType> ListAsync(int? count, int? offset, CancellationToken cancellationToken)
    {
        var query = scopes.AsQueryable().ApplyQuery(query => query.OrderBy(x => x.Id));
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

    public IAsyncEnumerable<TResult> ListAsync<TState, TResult>(Func<IQueryable<ScopeType>, TState, IQueryable<TResult>> query, TState state, CancellationToken cancellationToken)
    {
        return scopes
            .AsQueryable()
            .ApplyQuery(state, query)
            .ToAsyncEnumerable(cancellationToken);
    }

    public ValueTask<TResult?> GetAsync<TState, TResult>(Func<IQueryable<ScopeType>, TState, IQueryable<TResult>> query, TState state, CancellationToken cancellationToken)
    {
        var task = scopes
            .AsQueryable()
            .ApplyQuery(state, query)
            .FirstOrDefaultAsync(cancellationToken);
        return new(task!);
    }

    public ValueTask<ScopeType?> FindByIdAsync(string identifier, CancellationToken cancellationToken)
    {
        var filter = Builders<ScopeType>.Filter.Eq(x => x.Id, identifier);
        return new(scopes.Find(filter).FirstOrDefaultAsync(cancellationToken)!);
    }

    public ValueTask<ScopeType?> FindByNameAsync(string name, CancellationToken cancellationToken)
    {
        var filter = Builders<ScopeType>.Filter.Eq(x => x.Name.Default, name);
        return new(scopes.Find(filter).FirstOrDefaultAsync(cancellationToken)!);
    }

    public IAsyncEnumerable<ScopeType> FindByNamesAsync(ImmutableArray<string> names, CancellationToken cancellationToken)
    {
        // todo: Test
        var filter = Builders<ScopeType>.Filter.Where(x => x.Name.Values.Any(x => names.Contains(x)));
        return scopes.Find(filter).ToAsyncEnumerable(cancellationToken);
    }

    public IAsyncEnumerable<ScopeType> FindByResourceAsync(string resource, CancellationToken cancellationToken)
    {
        var filter = Builders<ScopeType>.Filter.AnyEq(x => x.Claims, resource);
        return scopes.Find(filter).ToAsyncEnumerable(cancellationToken);
    }

    #region CRUD

    public ValueTask<ScopeType> InstantiateAsync(CancellationToken cancellationToken)
    {
        return new(new ScopeType
        {
            Id = string.Empty,
            TimeAudit = new TimeAudit(timeProvider)
        });
    }

    public ValueTask CreateAsync(ScopeType scope, CancellationToken cancellationToken)
    {
        if (scope.Validate().Errors is { Count: > 0 } errors)
        {
            throw new ProblemException(Problems.Validation.WithValidationErrors(errors));
        }
        return new(scopes.InsertOneAsync(scope, null, cancellationToken));
    }

    public async ValueTask UpdateAsync(ScopeType scope, CancellationToken cancellationToken)
    {
        if (scope.Validate().Errors is { Count: > 0 } errors)
        {
            throw new ProblemException(Problems.Validation.WithValidationErrors(errors));
        }

        scope.UpdatedAt(timeProvider);

        var filter = Builders<ScopeType>.Filter.Eq(x => x.Id, scope.Id);
        var result = await scopes.ReplaceOneAsync(filter, scope, cancellationToken: cancellationToken);

        if (result.MatchedCount is 0)
        {
            throw new ConcurrencyException(SR.GetResourceString(SR.ID0245));
        }
    }

    public ValueTask DeleteAsync(ScopeType scope, CancellationToken cancellationToken)
    {
        var filter = Builders<ScopeType>.Filter.Eq(x => x.Id, scope.Id);
        return new(scopes.DeleteOneAsync(filter, cancellationToken));
    }

    #endregion

    #region Get Properties

    public ValueTask<string?> GetIdAsync(ScopeType scope, CancellationToken cancellationToken)
    {
        return new(scope.Id);
    }

    public ValueTask<string?> GetNameAsync(ScopeType scope, CancellationToken cancellationToken)
    {
        return new(scope.Id);
    }

    public ValueTask<string?> GetDisplayNameAsync(ScopeType scope, CancellationToken cancellationToken)
    {
        return new(scope.Name.Default);
    }

    public ValueTask<ImmutableDictionary<CultureInfo, string>> GetDisplayNamesAsync(ScopeType scope, CancellationToken cancellationToken)
    {
        return new(scope.Name.ToImmutableDictionary(
            pair => CultureInfo.GetCultureInfo(pair.Key),
            pair => pair.Value
        ));
    }

    public ValueTask<string?> GetDescriptionAsync(ScopeType scope, CancellationToken cancellationToken)
    {
        return new(scope.Description.Default);
    }

    public ValueTask<ImmutableDictionary<CultureInfo, string>> GetDescriptionsAsync(ScopeType scope, CancellationToken cancellationToken)
    {
        return new(scope.Description.ToImmutableDictionary(
            pair => CultureInfo.GetCultureInfo(pair.Key),
            pair => pair.Value
        ));
    }

    public ValueTask<ImmutableArray<string>> GetResourcesAsync(ScopeType scope, CancellationToken cancellationToken)
    {
        return new([.. scope.Claims]);
    }

    public ValueTask<ImmutableDictionary<string, JsonElement>> GetPropertiesAsync(ScopeType scope, CancellationToken cancellationToken)
    {
        var props = scope.Properties.ToImmutableDictionary();
        return new(props ?? ImmutableDictionary.Create<string, JsonElement>());
    }

    #endregion

    #region Set Properties

    public ValueTask SetNameAsync(ScopeType scope, string? name, CancellationToken cancellationToken)
    {
        scope.Id = name ?? string.Empty;
        return new();
    }

    public ValueTask SetDisplayNameAsync(ScopeType scope, string? name, CancellationToken cancellationToken)
    {
        scope.Name.Default = name;
        return new();
    }

    public ValueTask SetDisplayNamesAsync(ScopeType scope, ImmutableDictionary<CultureInfo, string> names, CancellationToken cancellationToken)
    {
        var defaultName = scope.Name.Default;
        scope.Name.Clear();
        scope.Name.Default = defaultName;
        foreach (var (culture, value) in names)
        {
            scope.Name[culture.Name] = value;
        }
        return new();
    }

    public ValueTask SetDescriptionAsync(ScopeType scope, string? description, CancellationToken cancellationToken)
    {
        scope.Description.Default = description;
        return new();
    }

    public ValueTask SetDescriptionsAsync(ScopeType scope, ImmutableDictionary<CultureInfo, string> descriptions, CancellationToken cancellationToken)
    {
        var defaultDesc = scope.Description.Default;
        scope.Description.Clear();
        scope.Description.Default = defaultDesc;
        foreach (var (culture, value) in descriptions)
        {
            scope.Description[culture.Name] = value;
        }
        return new();
    }

    public ValueTask SetResourcesAsync(ScopeType scope, ImmutableArray<string> resources, CancellationToken cancellationToken)
    {
        scope.Claims.Clear();
        scope.Claims.AddRange(resources);
        return new();
    }

    public ValueTask SetPropertiesAsync(ScopeType scope, ImmutableDictionary<string, JsonElement> properties, CancellationToken cancellationToken)
    {
        scope.Properties = new JsonObject().CopyFrom(properties);
        return new();
    }

    #endregion
}
