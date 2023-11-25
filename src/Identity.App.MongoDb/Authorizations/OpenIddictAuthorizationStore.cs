using Identity.Authorizations;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using OpenIddict.Abstractions;
using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Nodes;
using static OpenIddict.Abstractions.OpenIddictExceptions;
using SR = OpenIddict.Abstractions.OpenIddictResources;

namespace Identity.MongoDb.Authorizations;

public sealed class OpenIddictAuthorizationStore :
    IOpenIddictAuthorizationStoreResolver,
    IOpenIddictAuthorizationStore<Authorization>
{
    private readonly AuthorizationsAppModule module;
    private readonly TimeProvider timeProvider;
    private readonly IMongoCollection<Authorization> authorizations;

    public OpenIddictAuthorizationStore(AuthorizationsAppModule module, IdentityMongoDb mongoDb, TimeProvider timeProvider)
    {
        this.module = module;
        this.timeProvider = timeProvider;
        authorizations = mongoDb.GetCollection<Authorization>(module.AuthCollectionName);
    }

    public IOpenIddictAuthorizationStore<TAuthorization> Get<TAuthorization>() where TAuthorization : class
    {
        return (this as IOpenIddictAuthorizationStore<TAuthorization>)!;
    }

    public ValueTask<long> CountAsync(CancellationToken cancellationToken)
    {
        return new(authorizations.EstimatedDocumentCountAsync(null, cancellationToken));
    }

    public ValueTask<long> CountAsync<TResult>(Func<IQueryable<Authorization>, IQueryable<TResult>> query, CancellationToken cancellationToken)
    {
        var task = authorizations
            .AsQueryable()
            .ApplyQuery(query)
            .LongCountAsync(cancellationToken);
        return new(task);
    }

    public IAsyncEnumerable<Authorization> ListAsync(int? count, int? offset, CancellationToken cancellationToken)
    {
        var query = authorizations.AsQueryable().ApplyQuery(query => query.OrderBy(x => x.Id));
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

    public IAsyncEnumerable<TResult> ListAsync<TState, TResult>(Func<IQueryable<Authorization>, TState, IQueryable<TResult>> query, TState state, CancellationToken cancellationToken)
    {
        return authorizations
            .AsQueryable()
            .ApplyQuery(state, query)
            .ToAsyncEnumerable(cancellationToken);
    }

    public ValueTask<TResult?> GetAsync<TState, TResult>(Func<IQueryable<Authorization>, TState, IQueryable<TResult>> query, TState state, CancellationToken cancellationToken)
    {
        var task = authorizations
            .AsQueryable()
            .ApplyQuery(state, query)
            .FirstOrDefaultAsync(cancellationToken);
        return new(task!);
    }

    public IAsyncEnumerable<Authorization> FindAsync(string subject, string client, CancellationToken cancellationToken)
    {
        var filter = Builders<Authorization>.Filter.And(
            Builders<Authorization>.Filter.Eq(x => x.Subject, subject),
            Builders<Authorization>.Filter.Eq(x => x.ClientId, client)
        );
        return authorizations.Find(filter).ToAsyncEnumerable(cancellationToken);
    }

    public IAsyncEnumerable<Authorization> FindAsync(string subject, string client, string status, CancellationToken cancellationToken)
    {
        var filter = Builders<Authorization>.Filter.And(
            Builders<Authorization>.Filter.Eq(x => x.Subject, subject),
            Builders<Authorization>.Filter.Eq(x => x.ClientId, client),
            Builders<Authorization>.Filter.Eq(x => x.Status, status)
        );
        return authorizations.Find(filter).ToAsyncEnumerable(cancellationToken);
    }

    public IAsyncEnumerable<Authorization> FindAsync(string subject, string client, string status, string type, CancellationToken cancellationToken)
    {
        var filter = Builders<Authorization>.Filter.And(
            Builders<Authorization>.Filter.Eq(x => x.Subject, subject),
            Builders<Authorization>.Filter.Eq(x => x.ClientId, client),
            Builders<Authorization>.Filter.Eq(x => x.Status, status),
            Builders<Authorization>.Filter.Eq(x => x.Status, type)
        );
        return authorizations.Find(filter).ToAsyncEnumerable(cancellationToken);
    }

    public IAsyncEnumerable<Authorization> FindAsync(string subject, string client, string status, string type, ImmutableArray<string> scopes, CancellationToken cancellationToken)
    {
        var filter = Builders<Authorization>.Filter.And(
            Builders<Authorization>.Filter.Eq(x => x.Subject, subject),
            Builders<Authorization>.Filter.Eq(x => x.ClientId, client),
            Builders<Authorization>.Filter.Eq(x => x.Status, status),
            Builders<Authorization>.Filter.Eq(x => x.Status, type),
            Builders<Authorization>.Filter.All(x => x.Scopes, scopes)
        );
        return authorizations.Find(filter).ToAsyncEnumerable(cancellationToken);
    }

    public IAsyncEnumerable<Authorization> FindByApplicationIdAsync(string identifier, CancellationToken cancellationToken)
    {
        var filter = Builders<Authorization>.Filter.Eq(x => x.ClientId, identifier);
        return authorizations.Find(filter).ToAsyncEnumerable(cancellationToken);
    }

    public ValueTask<Authorization?> FindByIdAsync(string identifier, CancellationToken cancellationToken)
    {
        var filter = Builders<Authorization>.Filter.Eq(x => x.ClientId, identifier);
        return new(authorizations.Find(filter).FirstOrDefaultAsync(cancellationToken)!);
    }

    public IAsyncEnumerable<Authorization> FindBySubjectAsync(string subject, CancellationToken cancellationToken)
    {
        var filter = Builders<Authorization>.Filter.Eq(x => x.Subject, subject);
        return authorizations.Find(filter).ToAsyncEnumerable(cancellationToken);
    }

    public ValueTask PruneAsync(DateTimeOffset threshold, CancellationToken cancellationToken)
    {
        //var filter = Builders<Authorization>.Filter.And(
        //    Builders<Authorization>.Filter.Lt(x=>x.CreatedAt, threshold),
        //    Builders<Authorization>.Filter.Ne(x=>x.Status, Statuses.Valid)
        //);

        //var adHocFilter = Builders<Authorization>.Filter.And(
        //    Builders<Authorization>.Filter.Lt(x => x.CreatedAt, threshold),
        //    Builders<Authorization>.Filter.Eq(x => x.Type, AuthorizationTypes.AdHoc)
        //);

        ////Builders<Token>.Filter.Eq(x=>x.ClientId, )

        //authorizations.Aggregate()
        //    .Match(adHocFilter)
        //    .Project(x => new
        //    {
        //        Authorization = x,
        //        Tokens = Array.Empty<Token>()
        //    })
        //    .Lookup<Authorization>(
        //        tokensModule.CollectionName,
        //        new StringFieldDefinition<Authorization>(nameof(Authorization.Id)),
        //        new StringFieldDefinition<Token>(nameof(Token.AuthorizationId)),
        //        new RenderedFieldDefinition("authorizations")
        //    );
        // todo: Implement
        throw new NotImplementedException();
    }

    #region CRUD

    public ValueTask<Authorization> InstantiateAsync(CancellationToken cancellationToken)
    {
        return new(new Authorization
        {
            Id = Uuid.NewUuid(20),
            Type = string.Empty,
            Subject = string.Empty,
            ClientId = string.Empty,
            TimeAudit = new TimeAudit(timeProvider)
        });
    }

    public ValueTask CreateAsync(Authorization authorization, CancellationToken cancellationToken)
    {
        if (authorization.Validate().Errors is { Count: > 0 } errors)
        {
            throw new ProblemException(Problems.Validation.WithValidationErrors(errors));
        }
        return new(authorizations.InsertOneAsync(authorization, null, cancellationToken));
    }

    public async ValueTask UpdateAsync(Authorization authorization, CancellationToken cancellationToken)
    {
        if (authorization.Validate().Errors is { Count: > 0 } errors)
        {
            throw new ProblemException(Problems.Validation.WithValidationErrors(errors));
        }

        authorization.UpdatedAt(timeProvider);

        var filter = Builders<Authorization>.Filter.Eq(x => x.Id, authorization.Id);
        var result = await authorizations.ReplaceOneAsync(filter, authorization, cancellationToken: cancellationToken);

        if (result.MatchedCount is 0)
        {
            throw new ConcurrencyException(SR.GetResourceString(SR.ID0245));
        }
    }

    public ValueTask DeleteAsync(Authorization authorization, CancellationToken cancellationToken)
    {
        var filter = Builders<Authorization>.Filter.Eq(x => x.Id, authorization.Id);
        return new(authorizations.DeleteOneAsync(filter, cancellationToken));
    }

    #endregion

    #region Get Properties

    public ValueTask<string?> GetIdAsync(Authorization authorization, CancellationToken cancellationToken)
    {
        return new(authorization.Id);
    }

    public ValueTask<string?> GetTypeAsync(Authorization authorization, CancellationToken cancellationToken)
    {
        return new(authorization.Type);
    }

    public ValueTask<string?> GetSubjectAsync(Authorization authorization, CancellationToken cancellationToken)
    {
        return new(authorization.Subject);
    }

    public ValueTask<string?> GetApplicationIdAsync(Authorization authorization, CancellationToken cancellationToken)
    {
        return new(authorization.ClientId);
    }

    public ValueTask<string?> GetStatusAsync(Authorization authorization, CancellationToken cancellationToken)
    {
        return new(authorization.Status);
    }

    public ValueTask<DateTimeOffset?> GetCreationDateAsync(Authorization authorization, CancellationToken cancellationToken)
    {
        return new(authorization.TimeAudit.CreatedAt);
    }

    public ValueTask<ImmutableArray<string>> GetScopesAsync(Authorization authorization, CancellationToken cancellationToken)
    {
        return new([.. authorization.Scopes]);
    }

    public ValueTask<ImmutableDictionary<string, JsonElement>> GetPropertiesAsync(Authorization authorization, CancellationToken cancellationToken)
    {
        var props = authorization.Properties.ToImmutableDictionary();
        return new(props ?? ImmutableDictionary.Create<string, JsonElement>());
    }

    #endregion

    #region Set Properties

    public ValueTask SetTypeAsync(Authorization authorization, string? type, CancellationToken cancellationToken)
    {
        authorization.Type = type ?? string.Empty;
        return new();
    }

    public ValueTask SetSubjectAsync(Authorization authorization, string? subject, CancellationToken cancellationToken)
    {
        authorization.Subject = subject ?? string.Empty;
        return new();
    }

    public ValueTask SetApplicationIdAsync(Authorization authorization, string? identifier, CancellationToken cancellationToken)
    {
        authorization.ClientId = identifier ?? string.Empty;
        return new();
    }

    public ValueTask SetStatusAsync(Authorization authorization, string? status, CancellationToken cancellationToken)
    {
        authorization.Status = status ?? string.Empty;
        return new();
    }

    public ValueTask SetCreationDateAsync(Authorization authorization, DateTimeOffset? date, CancellationToken cancellationToken)
    {
        return new();
    }

    public ValueTask SetScopesAsync(Authorization authorization, ImmutableArray<string> scopes, CancellationToken cancellationToken)
    {
        authorization.Scopes.Clear();
        authorization.Scopes.AddRange(scopes);
        return new();
    }

    public ValueTask SetPropertiesAsync(Authorization authorization, ImmutableDictionary<string, JsonElement> properties, CancellationToken cancellationToken)
    {
        authorization.Properties = new JsonObject().CopyFrom(properties);
        return new();
    }

    #endregion
}
