using Core.MongoDb.Commons;
using Identity.Clients;
using Identity.Clients.Requests;
using MongoDB.Driver;

namespace Identity.MongoDb.Clients.Handlers;

public sealed class ListClientsHandler : QueryHandler<ListClients, ResultSet<Client>>
{
    private readonly ClientsAppModule module;
    private readonly IdentityMongoDb mongoDb;

    public ListClientsHandler(ClientsAppModule module, IdentityMongoDb mongoDb)
    {
        this.module = module;
        this.mongoDb = mongoDb;
    }

    public override async ValueTask<Result<ResultSet<Client>>> Handle(ListClients command, CancellationToken cancellationToken)
    {
        var collection = mongoDb.GetCollection<Client>(module.CollectionName);

        // todo: Implement filters.
        var filter = Builders<Client>.Filter.Empty;
        var count = await collection.EstimatedDocumentCountAsync(null, cancellationToken);
        var clients = await collection.Find(filter).ToListAsync(cancellationToken);

        return new ResultSet<Client>(count, clients);
    }
}
