using Core.MongoDb.Commons;
using Identity.Claims;
using Identity.Claims.Requests;
using MongoDB.Driver;

namespace Identity.ClaimTypes.Handlers;

public sealed class ListClaimTypesHandler : QueryHandler<ListClaimTypes, ResultSet<ClaimType>>
{
    private readonly ClaimTypesAppModule module;
    private readonly IdentityMongoDb mongoDb;

    public ListClaimTypesHandler(ClaimTypesAppModule module, IdentityMongoDb mongoDb)
    {
        this.module = module;
        this.mongoDb = mongoDb;
    }

    public override async ValueTask<Result<ResultSet<ClaimType>>> Handle(ListClaimTypes command, CancellationToken cancellationToken)
    {
        var collection = mongoDb.GetCollection<ClaimType>(module.CollectionName);

        // todo: Implement filters

        var filter = Builders<ClaimType>.Filter.Empty;

        var claimTypes = await collection.Find(filter).ToListAsync(cancellationToken);

        return new ResultSet<ClaimType>(claimTypes.Count, claimTypes);
    }
}
