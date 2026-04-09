namespace ShoppingMarket.Domain;

public class User
{
    public int Id { get; private set; }
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}