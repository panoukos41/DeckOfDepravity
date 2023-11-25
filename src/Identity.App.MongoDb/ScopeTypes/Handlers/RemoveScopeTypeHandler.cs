using Core.MongoDb.Commons;
using Identity.ScopeTypes;
using Identity.ScopeTypes.Requests;
using MongoDB.Driver;

namespace Identity.MongoDb.ScopeTypes.Handlers;
public sealed class RemoveScopeTypeHandler : CommandHandler<RemoveScopeType, Void>
{
    private readonly ScopeTypesAppModule module;
    private readonly IdentityMongoDb mongoDb;

    public RemoveScopeTypeHandler(ScopeTypesAppModule module, IdentityMongoDb mongoDb)
    {
        this.module = module;
        this.mongoDb = mongoDb;
    }

    public override async ValueTask<Result<Void>> Handle(RemoveScopeType command, CancellationToken cancellationToken)
    {
        var collection = mongoDb.GetCollection<ScopeType>(module.CollectionName);

        var filter = Builders<ScopeType>.Filter.Eq(x => x.Id, command.ScopeId);
        var result = await collection.DeleteOneAsync(filter, cancellationToken);

        if (result.DeletedCount is 1)
        {
            return Void.Value;
        }
        return Problems.NotFound;
    }
}

