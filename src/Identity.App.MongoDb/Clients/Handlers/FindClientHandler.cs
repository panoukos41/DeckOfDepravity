using Core.MongoDb.Commons;
using Identity.Clients;
using Identity.Clients.Requests;
using MongoDB.Driver;

namespace Identity.MongoDb.Clients.Handlers;

public sealed class FindClientHandler : QueryHandler<FindClient, Client>
{
    private readonly ClientsAppModule module;
    private readonly IdentityMongoDb mongoDb;

    public FindClientHandler(ClientsAppModule module, IdentityMongoDb mongoDb)
    {
        this.module = module;
        this.mongoDb = mongoDb;
    }

    public override async ValueTask<Result<Client>> Handle(FindClient command, CancellationToken cancellationToken)
    {
        var collection = mongoDb.GetCollection<Client>(module.CollectionName);

        var filter = Builders<Client>.Filter.Eq(x => x.Id, command.ClientId);

        var client = await collection.Find(filter).FirstOrDefaultAsync(cancellationToken);

        return client is { }
            ? client
            : Problems.NotFound;
    }
}
