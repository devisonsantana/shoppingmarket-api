namespace ShoppingMarket.Application.Services;
#pragma warning disable CS1591
public static class BCryptService
{
    public static string Hash(string text) => BCrypt.Net.BCrypt.HashPassword(text);
    public static bool Verify(string text, string hash) => BCrypt.Net.BCrypt.Verify(text, hash);
}
#pragma warning restore CS1591