using Core.MongoDb.Commons;
using MongoDB.Driver;

namespace Identity.MongoDb.Commons;

public sealed class IdentityMongoDb : MongoDbContext
{
    public override MongoClient Client { get; }

    public override IMongoDatabase Database { get; }

    public IdentityMongoDb(MongoClient client, string database)
    {
        Client = client;
        Database = client.GetDatabase(database);
    }
}
