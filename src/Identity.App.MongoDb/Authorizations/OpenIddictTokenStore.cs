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

public sealed class OpenIddictTokenStore :
    IOpenIddictTokenStoreResolver,
    IOpenIddictTokenStore<Token>
{
    private readonly IMongoCollection<Token> tokens;
    private readonly TimeProvider timeProvider;

    public OpenIddictTokenStore(AuthorizationsAppModule module, IdentityMongoDb mongoDb, TimeProvider timeProvider)
    {
        tokens = mongoDb.GetCollection<Token>(module.TokenCollectionName);
        this.timeProvider = timeProvider;
    }

    public IOpenIddictTokenStore<TToken> Get<TToken>() where TToken : class
    {
        return (this as IOpenIddictTokenStore<TToken>)!;
    }

    public ValueTask<long> CountAsync(CancellationToken cancellationToken)
    {
        return new(tokens.EstimatedDocumentCountAsync(null, cancellationToken));
    }

    public ValueTask<long> CountAsync<TResult>(Func<IQueryable<Token>, IQueryable<TResult>> query, CancellationToken cancellationToken)
    {
        var task = tokens
            .AsQueryable()
            .ApplyQuery(query)
            .LongCountAsync(cancellationToken);
        return new(task);
    }

    public IAsyncEnumerable<Token> ListAsync(int? count, int? offset, CancellationToken cancellationToken)
    {
        var query = tokens.AsQueryable().ApplyQuery(query => query.OrderBy(x => x.Id));
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

    public IAsyncEnumerable<TResult> ListAsync<TState, TResult>(Func<IQueryable<Token>, TState, IQueryable<TResult>> query, TState state, CancellationToken cancellationToken)
    {
        return tokens
            .AsQueryable()
            .ApplyQuery(state, query)
            .ToAsyncEnumerable(cancellationToken);
    }

    public ValueTask<TResult?> GetAsync<TState, TResult>(Func<IQueryable<Token>, TState, IQueryable<TResult>> query, TState state, CancellationToken cancellationToken)
    {
        var task = tokens
            .AsQueryable()
            .ApplyQuery(state, query)
            .FirstOrDefaultAsync(cancellationToken);
        return new(task!);
    }

    public IAsyncEnumerable<Token> FindAsync(string subject, string client, CancellationToken cancellationToken)
    {
        var filter = Builders<Token>.Filter.And(
            Builders<Token>.Filter.Eq(x => x.Subject, subject),
            Builders<Token>.Filter.Eq(x => x.ClientId, client)
        );
        return tokens.Find(filter).ToAsyncEnumerable(cancellationToken);
    }

    public IAsyncEnumerable<Token> FindAsync(string subject, string client, string status, CancellationToken cancellationToken)
    {
        var filter = Builders<Token>.Filter.And(
            Builders<Token>.Filter.Eq(x => x.Subject, subject),
            Builders<Token>.Filter.Eq(x => x.ClientId, client),
            Builders<Token>.Filter.Eq(x => x.Status, status)
        );
        return tokens.Find(filter).ToAsyncEnumerable(cancellationToken);
    }

    public IAsyncEnumerable<Token> FindAsync(string subject, string client, string status, string type, CancellationToken cancellationToken)
    {
        var filter = Builders<Token>.Filter.And(
            Builders<Token>.Filter.Eq(x => x.Subject, subject),
            Builders<Token>.Filter.Eq(x => x.ClientId, client),
            Builders<Token>.Filter.Eq(x => x.Status, status),
            Builders<Token>.Filter.Eq(x => x.Type, type)
        );
        return tokens.Find(filter).ToAsyncEnumerable(cancellationToken);
    }

    public IAsyncEnumerable<Token> FindBySubjectAsync(string subject, CancellationToken cancellationToken)
    {
        var filter = Builders<Token>.Filter.Eq(x => x.Subject, subject);
        return tokens.Find(filter).ToAsyncEnumerable(cancellationToken);
    }

    public IAsyncEnumerable<Token> FindByApplicationIdAsync(string identifier, CancellationToken cancellationToken)
    {
        var filter = Builders<Token>.Filter.Eq(x => x.ClientId, identifier);
        return tokens.Find(filter).ToAsyncEnumerable(cancellationToken);
    }

    public IAsyncEnumerable<Token> FindByAuthorizationIdAsync(string identifier, CancellationToken cancellationToken)
    {
        var filter = Builders<Token>.Filter.Eq(x => x.AuthorizationId, identifier);
        return tokens.Find(filter).ToAsyncEnumerable(cancellationToken);
    }

    public ValueTask<Token?> FindByIdAsync(string identifier, CancellationToken cancellationToken)
    {
        var filter = Builders<Token>.Filter.Eq(x => x.Id, identifier);
        return new(tokens.Find(filter).FirstOrDefault(cancellationToken));
    }

    public ValueTask<Token?> FindByReferenceIdAsync(string identifier, CancellationToken cancellationToken)
    {
        var filter = Builders<Token>.Filter.Eq(x => x.ReferenceId, identifier);
        return new(tokens.Find(filter).FirstOrDefault(cancellationToken));
    }

    public ValueTask PruneAsync(DateTimeOffset threshold, CancellationToken cancellationToken)
    {
        // todo: Implement
        throw new NotImplementedException();
    }

    #region CRUD

    public ValueTask<Token> InstantiateAsync(CancellationToken cancellationToken)
    {
        return new(new Token
        {
            Id = Uuid.NewUuid(20),
            Type = string.Empty,
            Subject = string.Empty,
            ClientId = string.Empty,
            AuthorizationId = string.Empty,
            Status = string.Empty,
            TimeAudit = new TimeAudit(timeProvider)
        });
    }

    public ValueTask CreateAsync(Token token, CancellationToken cancellationToken)
    {
        if (token.Validate().Errors is { Count: > 0 } errors)
        {
            throw new ProblemException(Problems.Validation.WithValidationErrors(errors));
        }
        return new(tokens.InsertOneAsync(token, null, cancellationToken));
    }

    public async ValueTask UpdateAsync(Token token, CancellationToken cancellationToken)
    {
        if (token.Validate().Errors is { Count: > 0 } errors)
        {
            throw new ProblemException(Problems.Validation.WithValidationErrors(errors));
        }

        token.UpdatedAt(timeProvider);

        var filter = Builders<Token>.Filter.Eq(x => x.Id, token.Id);
        var result = await tokens.ReplaceOneAsync(filter, token, cancellationToken: cancellationToken);

        if (result.MatchedCount is 0)
        {
            throw new ConcurrencyException(SR.GetResourceString(SR.ID0245));
        }
    }

    public ValueTask DeleteAsync(Token token, CancellationToken cancellationToken)
    {
        var filter = Builders<Token>.Filter.Eq(x => x.Id, token.Id);
        return new(tokens.DeleteOneAsync(filter, cancellationToken));
    }

    #endregion

    #region Get Properties

    public ValueTask<string?> GetIdAsync(Token token, CancellationToken cancellationToken)
    {
        return new(token.Id);
    }

    public ValueTask<string?> GetTypeAsync(Token token, CancellationToken cancellationToken)
    {
        return new(token.Type);
    }

    public ValueTask<string?> GetSubjectAsync(Token token, CancellationToken cancellationToken)
    {
        return new(token.Subject);
    }

    public ValueTask<string?> GetApplicationIdAsync(Token token, CancellationToken cancellationToken)
    {
        return new(token.ClientId);
    }

    public ValueTask<string?> GetAuthorizationIdAsync(Token token, CancellationToken cancellationToken)
    {
        return new(token.AuthorizationId);
    }

    public ValueTask<string?> GetStatusAsync(Token token, CancellationToken cancellationToken)
    {
        return new(token.Status);
    }

    public ValueTask<DateTimeOffset?> GetCreationDateAsync(Token token, CancellationToken cancellationToken)
    {
        return new(token.TimeAudit.CreatedAt);
    }

    public ValueTask<DateTimeOffset?> GetRedemptionDateAsync(Token token, CancellationToken cancellationToken)
    {
        return new(token.RedeemedAt);
    }

    public ValueTask<DateTimeOffset?> GetExpirationDateAsync(Token token, CancellationToken cancellationToken)
    {
        return new(token.ExpiresAt);
    }

    public ValueTask<ImmutableDictionary<string, JsonElement>> GetPropertiesAsync(Token token, CancellationToken cancellationToken)
    {
        var props = token.Properties.ToImmutableDictionary();
        return new(props ?? ImmutableDictionary.Create<string, JsonElement>());
    }

    public ValueTask<string?> GetPayloadAsync(Token token, CancellationToken cancellationToken)
    {
        return new(token.Payload);
    }

    public ValueTask<string?> GetReferenceIdAsync(Token token, CancellationToken cancellationToken)
    {
        return new(token.ReferenceId);
    }

    #endregion

    #region Set Properties

    public ValueTask SetTypeAsync(Token token, string? type, CancellationToken cancellationToken)
    {
        token.Type = type ?? string.Empty;
        return new();
    }

    public ValueTask SetSubjectAsync(Token token, string? subject, CancellationToken cancellationToken)
    {
        token.Subject = subject ?? string.Empty;
        return new();
    }

    public ValueTask SetApplicationIdAsync(Token token, string? identifier, CancellationToken cancellationToken)
    {
        token.ClientId = identifier ?? string.Empty;
        return new();
    }

    public ValueTask SetAuthorizationIdAsync(Token token, string? identifier, CancellationToken cancellationToken)
    {
        token.AuthorizationId = identifier ?? string.Empty;
        return new();
    }

    public ValueTask SetStatusAsync(Token token, string? status, CancellationToken cancellationToken)
    {
        token.Status = status;
        return new();
    }

    public ValueTask SetCreationDateAsync(Token token, DateTimeOffset? date, CancellationToken cancellationToken)
    {
        return new();
    }

    public ValueTask SetRedemptionDateAsync(Token token, DateTimeOffset? date, CancellationToken cancellationToken)
    {
        token.RedeemedAt = date;
        return new();
    }

    public ValueTask SetExpirationDateAsync(Token token, DateTimeOffset? date, CancellationToken cancellationToken)
    {
        token.ExpiresAt = date;
        return new();
    }

    public ValueTask SetPropertiesAsync(Token token, ImmutableDictionary<string, JsonElement> properties, CancellationToken cancellationToken)
    {
        token.Properties = new JsonObject().CopyFrom(properties);
        return new();
    }

    public ValueTask SetPayloadAsync(Token token, string? payload, CancellationToken cancellationToken)
    {
        token.Payload = payload;
        return new();
    }

    public ValueTask SetReferenceIdAsync(Token token, string? identifier, CancellationToken cancellationToken)
    {
        token.ReferenceId = identifier;
        return new();
    }

    #endregion
}
