using Core.MongoDb.Commons;
using Identity.RoleTypes;
using Identity.RoleTypes.Requests;
using MongoDB.Driver;

namespace Identity.MongoDb.RoleTypes.Handlers;

public sealed class ListRoleTypesHandler : QueryHandler<ListRoleTypes, ResultSet<RoleType>>
{
    private readonly RoleTypesAppModule module;
    private readonly IdentityMongoDb mongoDb;

    public ListRoleTypesHandler(RoleTypesAppModule module, IdentityMongoDb mongoDb)
    {
        this.module = module;
        this.mongoDb = mongoDb;
    }

    public override async ValueTask<Result<ResultSet<RoleType>>> Handle(ListRoleTypes query, CancellationToken cancellationToken)
    {
        var collection = mongoDb.GetCollection<RoleType>(module.CollectionName);

        // todo: Implement filters

        var filter = Builders<RoleType>.Filter.Empty;

        var roles = await collection.Find(filter).ToListAsync(cancellationToken);

        return new ResultSet<RoleType>(roles.Count, roles);
    }
}
