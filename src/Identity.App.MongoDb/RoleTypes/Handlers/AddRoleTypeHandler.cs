using Core.MongoDb.Commons;
using Identity.RoleTypes;
using Identity.RoleTypes.Requests;
using MongoDB.Driver;

namespace Identity.MongoDb.RoleTypes.Handlers;

public sealed class AddRoleTypeHandler : CommandHandler<AddRoleType, Void>
{
    private readonly RoleTypesAppModule module;
    private readonly IdentityMongoDb mongoDb;

    public AddRoleTypeHandler(RoleTypesAppModule module, IdentityMongoDb mongoDb)
    {
        this.module = module;
        this.mongoDb = mongoDb;
    }

    public override async ValueTask<Result<Void>> Handle(AddRoleType command, CancellationToken cancellationToken)
    {
        var collection = mongoDb.GetCollection<RoleType>(module.CollectionName);

        var filter = Builders<RoleType>.Filter.Eq(x => x.Name, command.Data.Name);
        var exists = await collection.Find(filter).AnyAsync(cancellationToken);

        if (exists)
        {
            return Problems.Conflict;
        }

        await collection.InsertOneAsync(command.Data, null, cancellationToken);

        return Void.Value;
    }
}
