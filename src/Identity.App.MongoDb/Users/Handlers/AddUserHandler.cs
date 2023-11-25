using Core.MongoDb.Commons;
using Identity.Users;
using Identity.Users.Requests;
using MongoDB.Driver;

namespace Identity.MongoDb.Users.Handlers;

public sealed class AddUserHandler : CommandHandler<AddUser, CreatedResponse>
{
    private readonly UsersAppModule module;
    private readonly IdentityMongoDb mongoDb;

    public AddUserHandler(UsersAppModule module, IdentityMongoDb mongoDb)
    {
        this.module = module;
        this.mongoDb = mongoDb;
    }

    private static readonly UpdateOptions options = new() { IsUpsert = true };

    public override async ValueTask<Result<CreatedResponse>> Handle(AddUser command, CancellationToken cancellationToken)
    {
        var collection = mongoDb.GetCollection<User>(module.CollectionName);
        var user = command.Data with { Id = Uuid.NewUuid() };

        var filter = Builders<User>.Filter.Or(
            Builders<User>.Filter.Eq(x => x.Email, user.Email),
            Builders<User>.Filter.Eq(x => x.Username, user.Username)
        );

        var exists = await collection.Find(filter).AnyAsync(cancellationToken);

        if (exists)
        {
            return Problems.Conflict;
        }

        await collection.InsertOneAsync(user, null, cancellationToken);

        return new CreatedResponse(user.Id);
    }
}
