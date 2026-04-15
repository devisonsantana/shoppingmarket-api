namespace ShoppingMarket.Application.DTOs;

public record UserCreateDTO
{
    public string Username { get; init; }
    public string Email { get; init; }
    public string Password { get; init; }
    public UserCreateDTO(string username, string email, string password)
    {
        Username = username;
        Email = email;
        Password = password;
    }
}
public record UserLoginDTO
{
    public string Email { get; init; }
    public string Password { get; init; }
    public UserLoginDTO(string email, string password)
    {
        Email = email;
        Password = password;
    }
}
public record UserResponseDTO
{
    public string Id { get; init; }
    public string Username { get; init; } = null!;
    public string Email { get; init; } = null!;
    public UserResponseDTO(string id, string username, string email)
    {
        Id = id;
        Username = username;
        Email = email;
    }
}
public record UserAccessDTO
{
    public string AccessToken { get; init; }
    public UserResponseDTO User { get; init; }
    public UserAccessDTO(string accessToken, UserResponseDTO user)
    {
        AccessToken = accessToken;
        User = user;
    }
}