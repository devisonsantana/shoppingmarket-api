using ShoppingMarket.Application.DTOs;
using ShoppingMarket.Infrastructure.Data;
using ShoppingMarket.Domain;
using Microsoft.AspNetCore.Http.HttpResults;
using ShoppingMarket.Application.Services;
using ShoppingMarket.Infrastructure.OpenAPI;
using ShoppingMarket.Infrastructure.Security;
using MongoDB.Driver;
using MongoDB.Bson;

namespace ShoppingMarket;

public class Program
{
    private static string? _secretKey { get; set; }
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var corsPolicyName = "FrontEndCorsPolicy";

        _secretKey = builder.Configuration.GetValue<string>("BearerKey") ?? throw new InvalidOperationException("Token Key is not configured");

        // MongoDb configuration
        builder.Services.Configure<DataStoreSettings>(builder.Configuration.GetSection("DatabaseSettings"));
        builder.Services.AddSingleton<MongoDbService>();

        builder.Services.AddCors(options =>
        {
            var allowedHosts = builder.Configuration.GetValue<string>("AllowedHosts") ?? throw new InvalidOperationException("No allowed host is configured");
            options.AddPolicy(corsPolicyName, policy => policy.WithOrigins(allowedHosts).AllowAnyMethod().WithHeaders("Authorization", "Content-Type"));
        });

        builder.Services.AddJwtAuthentication(_secretKey);

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddOpenApi();
        builder.Services.AddSwaggerWithAuth();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseCors(corsPolicyName);
        app.UseAuthentication();
        app.UseAuthorization();

        var productItems = app.MapGroup("/products").WithTags("Products");

        productItems.MapGet("/", GetAllProducts);

        productItems.MapGet("/{id:length(24)}", GetProductById);

        productItems.MapPost("/", CreateProduct).RequireAuthorization();

        productItems.MapPost("/bulk", CreateProductsByList).RequireAuthorization();

        app.MapPost("/login", Login).WithTags("Users");

        app.MapPost("/users", Register).WithTags("Users");

        app.MapGet("/users/{id:length(24)}", GetUserById).WithTags("Users").RequireAuthorization();

        app.Run();
    }

    #region User Methods
    static async Task<Results<Ok<UserResponseDTO>, NotFound>> GetUserById(MongoDbService mongo, string id)
    {
        var user = await mongo.Users.Find(u => u.Id == id).FirstOrDefaultAsync();
        return user is not null ? TypedResults.Ok(new UserResponseDTO(user.Id!, user.Username, user.Email)) : TypedResults.NotFound();
    }
    static async Task<Results<Created<UserResponseDTO>, Conflict>> Register(MongoDbService mongo, UserCreateDTO dto)
    {
        var user = await mongo.Users.Find(u => u.Email == dto.Email).FirstOrDefaultAsync();
        if (user is not null)
            return TypedResults.Conflict();

        var encode = BCryptService.Hash;

        var passwordHash = encode(dto.Password);
        user = new User()
        {
            Username = dto.Username,
            Email = dto.Email,
            Password = passwordHash
        };

        await mongo.Users.InsertOneAsync(user);

        var responseUser = new UserResponseDTO(user.Id!, user.Username, user.Email);
        return TypedResults.Created($"/users/{responseUser.Id}", responseUser);

    }
    static async Task<Results<Ok<UserAccessDTO>, UnauthorizedHttpResult>> Login(MongoDbService mongo, UserLoginDTO dto)
    {
        var user = await mongo.Users.Find(u => u.Email == dto.Email).FirstOrDefaultAsync();
        if (user is null)
            return TypedResults.Unauthorized();

        var isValid = BCryptService.Verify;

        if (!isValid(dto.Password, user.Password))
            return TypedResults.Unauthorized();

        var accessToken = TokenService.GenerateToken(user, _secretKey!);
        return TypedResults.Ok(new UserAccessDTO(accessToken, new UserResponseDTO(user.Id!, user.Username, user.Email)));
    }
    #endregion

    #region Products Methods
    static async Task<Ok<List<Product>>> GetAllProducts(MongoDbService mongo, string? name, decimal? price, string? category, string? sortBy, string? sortOrder)
    {
        var builder = Builders<Product>.Filter;
        var filter = builder.Empty;

        if (!string.IsNullOrEmpty(name))
            filter &= builder.Regex(p => p.Name, new BsonRegularExpression(name, "i"));

        if (price.HasValue)
            filter &= builder.Eq(p => p.Price, price.Value);

        if (!string.IsNullOrEmpty(category))
            filter &= builder.Eq(p => p.Category, category);

        var isDescending = sortOrder?.ToLower() == "desc";

        var sort = sortBy?.ToLower() switch
        {
            "name" => isDescending ? Builders<Product>.Sort.Descending(p => p.Name) : Builders<Product>.Sort.Ascending(p => p.Name),
            "price" => isDescending ? Builders<Product>.Sort.Descending(p => p.Price) : Builders<Product>.Sort.Ascending(p => p.Price),
            "category" => isDescending ? Builders<Product>.Sort.Descending(p => p.Category) : Builders<Product>.Sort.Ascending(p => p.Category),
            _ => isDescending ? Builders<Product>.Sort.Descending(p => p.Id) : Builders<Product>.Sort.Ascending(p => p.Id)
        };

        var options = new FindOptions
        {
            Collation = new Collation("pt", strength: CollationStrength.Primary)
        };

        var products = await mongo.Products
            .Find(filter, options)
            .Sort(sort)
            .ToListAsync();

        return TypedResults.Ok(products);
    }
    static async Task<Results<Ok<Product>, NotFound>> GetProductById(MongoDbService mongo, string id) => await mongo.Products.Find(p => p.Id == id).FirstOrDefaultAsync() is Product product ? TypedResults.Ok<Product>(product) : TypedResults.NotFound();
    static async Task<Created<Product>> CreateProduct(MongoDbService mongo, ProductDTO dto)
    {
        var product = new Product
        {
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            Quantity = dto.Quantity,
            Category = dto.Category,
            Image = dto.Image
        };

        await mongo.Products.InsertOneAsync(product);
        return TypedResults.Created($"/products/{product.Id}", product);
    }
    static async Task<Ok<string>> CreateProductsByList(MongoDbService mongo, List<ProductDTO> dtos)
    {
        var products = dtos.Select(p => new Product
        {
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            Quantity = p.Quantity,
            Category = p.Category,
            Image = p.Image
        }).ToList();

        await mongo.Products.InsertManyAsync(products);
        return TypedResults.Ok($"{products.Count} products added");
    }
    #endregion
}
