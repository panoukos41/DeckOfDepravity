using Core.MongoDb.Commons;
using Identity.MongoDb.RoleTypes;
using Identity.RoleTypes.Requests;
using MongoDB.Driver;
using Identity.ScopeTypes.Requests;
using Identity.ScopeTypes;

namespace Identity.MongoDb.ScopeTypes.Handlers;

public sealed class AddScopeTypeHandler : CommandHandler<AddScopeType, Void>
{
    private readonly ScopeTypesAppModule module;
    private readonly IdentityMongoDb mongoDb;

    public AddScopeTypeHandler(ScopeTypesAppModule module, IdentityMongoDb mongoDb)
    {
        this.module = module;
        this.mongoDb = mongoDb;
    }

    public override async ValueTask<Result<Void>> Handle(AddScopeType command, CancellationToken cancellationToken)
    {
        var collection = mongoDb.GetCollection<ScopeType>(module.CollectionName);

        var filter = Builders<ScopeType>.Filter.Eq(x => x.Id, command.Data.Id);
        var exists = await collection.Find(filter).AnyAsync(cancellationToken);

        if (exists)
        {
            return Problems.Conflict;
        }

        await collection.InsertOneAsync(command.Data, null, cancellationToken);

        return Void.Value;
    }
}
