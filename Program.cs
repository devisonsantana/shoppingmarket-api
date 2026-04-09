using Microsoft.EntityFrameworkCore;
using ShoppingMarket.Application.DTOs;
using ShoppingMarket.Infrastructure.Data;
using ShoppingMarket.Domain;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ShoppingMarket.Application.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using ShoppingMarket.Infrastructure.OpenAPI;
using ShoppingMarket.Infrastructure.Security;

namespace ShoppingMarket;

public class Program
{
    private static string? _secretKey { get; set; }
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var corsPolicyName = "FrontEndCorsPolicy";

        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("There is an error in Database Connection String");

        _secretKey = builder.Configuration.GetValue<string>("BearerKey") ?? throw new InvalidOperationException("Token Key is not configured");

        builder.Services.AddDbContext<DataContext>(options => options.UseSqlServer(connectionString));

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

        productItems.MapGet("/{id:int}", GetProductById);

        productItems.MapPost("/", CreateProduct).RequireAuthorization();

        productItems.MapPost("/bulk", CreateProductsByList).RequireAuthorization();

        app.MapPost("/login", Login).WithTags("Users");

        app.MapPost("/users", Register).WithTags("Users");

        app.MapGet("/users/{id:int}", GetUserById).WithTags("Users").RequireAuthorization();

        app.UseHttpsRedirection();

        app.Run();
    }

    #region User Methods
    static async Task<Results<Ok<UserResponseDTO>, NotFound>> GetUserById(DataContext db, int id)
    {
        return await db.Users.FindAsync(id) is User user ? TypedResults.Ok(new UserResponseDTO(user.Id, user.Username, user.Email)) : TypedResults.NotFound();
    }
    static async Task<Results<Created<UserResponseDTO>, Conflict>> Register(DataContext db, UserCreateDTO dto)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
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

        db.Users.Add(user);
        await db.SaveChangesAsync();

        var responseUser = new UserResponseDTO(user.Id, user.Username, user.Email);
        return TypedResults.Created($"/users/{responseUser.Id}", responseUser);

    }
    static async Task<Results<Ok<UserAccessDTO>, UnauthorizedHttpResult>> Login(DataContext db, UserLoginDTO dto)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user is null)
            return TypedResults.Unauthorized();

        var isValid = BCryptService.Verify;

        if (!isValid(dto.Password, user.Password))
            return TypedResults.Unauthorized();

        var accessToken = TokenService.GenerateToken(user, _secretKey!);
        return TypedResults.Ok(new UserAccessDTO(accessToken, new UserResponseDTO(user.Id, user.Username, user.Email)));
    }
    #endregion

    #region Products Methods
    static async Task<Ok<List<Product>>> GetAllProducts(DataContext db, string? name, decimal? price, string? category, string? sortBy, string? sortOrder)
    {
        var query = db.Products.AsQueryable();
        if (!string.IsNullOrEmpty(name))
            query = query.Where(p => p.Name.Contains(name));

        if (price.HasValue)
            query = query.Where(p => p.Price == price.Value);

        if (!string.IsNullOrEmpty(category))
            query = query.Where(p => p.Category == category);

        var isDescending = sortOrder?.ToLower() == "desc";
        query = sortBy?.ToLower() switch
        {
            "name" => isDescending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
            "price" => isDescending ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price),
            "category" => isDescending ? query.OrderByDescending(p => p.Category) : query.OrderBy(p => p.Category),
            _ => isDescending ? query.OrderByDescending(p => p.Id) : query.OrderBy(p => p.Id)
        };

        var products = await query.ToListAsync();
        return TypedResults.Ok(products);
    }
    static async Task<Results<Ok<Product>, NotFound>> GetProductById(DataContext db, int id) => await db.Products.FindAsync(id) is Product product ? TypedResults.Ok<Product>(product) : TypedResults.NotFound();
    static async Task<Created<Product>> CreateProduct(DataContext db, ProductDTO dto)
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
        db.Products.Add(product);
        await db.SaveChangesAsync();
        return TypedResults.Created($"/products/{product.Id}", product);
    }
    static async Task<Ok<string>> CreateProductsByList(DataContext db, List<ProductDTO> dtos)
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
        db.Products.AddRange(products);
        await db.SaveChangesAsync();
        return TypedResults.Ok($"{products.Count} products added");
    }
    #endregion
}
