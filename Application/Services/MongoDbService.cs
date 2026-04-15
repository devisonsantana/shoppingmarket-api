using Microsoft.Extensions.Options;
using MongoDB.Driver;
using ShoppingMarket.Domain;
using ShoppingMarket.Infrastructure.Data;

namespace ShoppingMarket.Application.Services;

public class MongoDbService
{
    public IMongoCollection<User> Users { get; }
    public IMongoCollection<Product> Products { get; }
    public MongoDbService(IOptions<DataStoreSettings> settings)
    {
        var mongoClient = new MongoClient(settings.Value.ConnectionString);
        var mongoDatabase = mongoClient.GetDatabase(settings.Value.DatabaseName);
        Users = mongoDatabase.GetCollection<User>(settings.Value.UsersCollectionName);
        Products = mongoDatabase.GetCollection<Product>(settings.Value.ProductsCollectionName);
    }
}