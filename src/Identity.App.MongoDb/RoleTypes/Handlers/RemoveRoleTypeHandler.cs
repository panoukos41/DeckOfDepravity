using Core.MongoDb.Commons;
using Identity.RoleTypes;
using Identity.RoleTypes.Requests;
using MongoDB.Driver;

namespace Identity.MongoDb.RoleTypes.Handlers;

public sealed class RemoveRoleTypeHandler : CommandHandler<RemoveRoleType, Void>
{
    private readonly RoleTypesAppModule module;
    private readonly IdentityMongoDb mongoDb;

    public RemoveRoleTypeHandler(RoleTypesAppModule module, IdentityMongoDb mongoDb)
    {
        this.module = module;
        this.mongoDb = mongoDb;
    }

    public override async ValueTask<Result<Void>> Handle(RemoveRoleType command, CancellationToken cancellationToken)
    {
        var collection = mongoDb.GetCollection<RoleType>(module.CollectionName);

        var filter = Builders<RoleType>.Filter.Eq(x => x.Name, command.Name);
        var result = await collection.DeleteOneAsync(filter, cancellationToken);

        if (result.DeletedCount is 1)
        {
            return Void.Value;
        }
        return Problems.NotFound;
    }
}
