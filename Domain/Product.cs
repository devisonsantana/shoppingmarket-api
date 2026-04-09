using System.ComponentModel.DataAnnotations.Schema;

namespace ShoppingMarket.Domain;

public class Product
{
    public int Id { get; private set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    [Column(TypeName = "decimal(6,2)")]
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public string Category { get; set; } = null!;
    public string Image { get; set; } = null!;
}