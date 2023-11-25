using Core.MongoDb.Commons;
using Identity.Claims;
using Identity.Claims.Requests;
using MongoDB.Driver;

namespace Identity.ClaimTypes.Handlers;

public sealed class AddClaimTypeHandler : CommandHandler<AddClaimType, Void>
{
    private readonly ClaimTypesAppModule module;
    private readonly IdentityMongoDb mongoDb;

    public AddClaimTypeHandler(ClaimTypesAppModule module, IdentityMongoDb mongoDb)
    {
        this.module = module;
        this.mongoDb = mongoDb;
    }

    public override async ValueTask<Result<Void>> Handle(AddClaimType command, CancellationToken cancellationToken)
    {
        var collection = mongoDb.GetCollection<ClaimType>(module.CollectionName);

        var filter = Builders<ClaimType>.Filter.Eq(x => x.Type, command.Data.Type);
        var exists = await collection.Find(filter).AnyAsync(cancellationToken);

        if (exists)
        {
            return Problems.Conflict;
        }

        await collection.InsertOneAsync(command.Data, null, cancellationToken);

        return Void.Value;
    }
}
