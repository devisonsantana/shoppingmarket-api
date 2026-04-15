using System.ComponentModel.DataAnnotations.Schema;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ShoppingMarket.Domain;

public class Product
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; private set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    [Column(TypeName = "decimal(6,2)")]
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public string Category { get; set; } = null!;
    public string Image { get; set; } = null!;
}