using Core.MongoDb.Commons;
using Identity.ScopeTypes;
using Identity.ScopeTypes.Requests;
using MongoDB.Driver;

namespace Identity.MongoDb.ScopeTypes.Handlers;

public sealed class ListScopesTypesHandler : QueryHandler<ListScopeTypes, ResultSet<ScopeType>>
{
    private readonly ScopeTypesAppModule module;
    private readonly IdentityMongoDb mongoDb;

    public ListScopesTypesHandler(ScopeTypesAppModule module, IdentityMongoDb mongoDb)
    {
        this.module = module;
        this.mongoDb = mongoDb;
    }

    public override async ValueTask<Result<ResultSet<ScopeType>>> Handle(ListScopeTypes query, CancellationToken cancellationToken)
    {
        var collection = mongoDb.GetCollection<ScopeType>(module.CollectionName);

        var filter = Builders<ScopeType>.Filter.Empty;

        var scopes = await collection.Find(filter).ToListAsync(cancellationToken);

        return new ResultSet<ScopeType>(scopes.Count, scopes);
    }
}
