using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ShoppingMarket.Domain;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; private set; }
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}