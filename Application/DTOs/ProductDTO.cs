namespace ShoppingMarket.Application.DTOs;

public record ProductDTO
{
    public string Name { get; init; }
    public string Description { get; init; }
    public decimal Price { get; init; }
    public int Quantity { get; init; }
    public string Category { get; init; }
    public string Image { get; init; }
    public ProductDTO(string name, string description, decimal price, int quantity, string category, string image)
    {
        Name = name;
        Description = description;
        Price = price;
        Quantity = quantity;
        Category = category;
        Image = image;
    }
}