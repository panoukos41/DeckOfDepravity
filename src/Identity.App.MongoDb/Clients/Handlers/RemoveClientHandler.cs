using Core.MongoDb.Commons;
using Identity.Clients;
using Identity.Clients.Requests;
using MongoDB.Driver;

namespace Identity.MongoDb.Clients.Handlers;

public sealed class RemoveClientHandler : CommandHandler<RemoveClient, Void>
{
    private readonly ClientsAppModule module;
    private readonly IdentityMongoDb mongoDb;

    public RemoveClientHandler(ClientsAppModule module, IdentityMongoDb mongoDb)
    {
        this.module = module;
        this.mongoDb = mongoDb;
    }

    public override async ValueTask<Result<Void>> Handle(RemoveClient command, CancellationToken cancellationToken)
    {
        var collection = mongoDb.GetCollection<Client>(module.CollectionName);

        var filter = Builders<Client>.Filter.Eq(x => x.Id, command.ClientId);
        var result = await collection.DeleteOneAsync(filter, cancellationToken);

        return result.IsAcknowledged && result.DeletedCount is 1
            ? (Result<Void>)Void.Value
            : (Result<Void>)Problems.NotFound;
    }
}
