using Core.MongoDb.Commons;
using Identity.Claims;
using Identity.Claims.Requests;
using MongoDB.Driver;

namespace Identity.ClaimTypes.Handlers;

public sealed class RemoveClaimTypeHandler : CommandHandler<RemoveClaimType, Void>
{
    private readonly ClaimTypesAppModule module;
    private readonly IdentityMongoDb mongoDb;

    public RemoveClaimTypeHandler(ClaimTypesAppModule module, IdentityMongoDb mongoDb)
    {
        this.module = module;
        this.mongoDb = mongoDb;
    }

    public override async ValueTask<Result<Void>> Handle(RemoveClaimType command, CancellationToken cancellationToken)
    {
        var collection = mongoDb.GetCollection<ClaimType>(module.CollectionName);

        var filter = Builders<ClaimType>.Filter.Eq(x => x.Type, command.Type);
        var result = await collection.DeleteOneAsync(filter, cancellationToken);

        return result.DeletedCount is 1
            ? Void.Value
            : Problems.NotFound;
    }
}
