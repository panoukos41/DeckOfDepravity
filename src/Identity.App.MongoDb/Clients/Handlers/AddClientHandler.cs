using Core.MongoDb.Commons;
using Identity.Clients;
using Identity.Clients.Requests;
using MongoDB.Driver;

namespace Identity.MongoDb.Clients.Handlers;

public sealed class AddClientHandler : CommandHandler<AddClient, Void>
{
    private readonly ClientsAppModule module;
    private readonly IdentityMongoDb mongoDb;

    public AddClientHandler(ClientsAppModule module, IdentityMongoDb mongoDb)
    {
        this.module = module;
        this.mongoDb = mongoDb;
    }

    public override async ValueTask<Result<Void>> Handle(AddClient command, CancellationToken cancellationToken)
    {
        var collection = mongoDb.GetCollection<Client>(module.CollectionName);

        var filter = Builders<Client>.Filter.Eq(x => x.Id, command.Data.Id);
        var exists = await collection.Find(filter).AnyAsync(cancellationToken);

        if (exists)
        {
            return Problems.Conflict;
        }

        await collection.InsertOneAsync(command.Data, null, cancellationToken);

        return Void.Value;
    }
}
