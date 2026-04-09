using Microsoft.EntityFrameworkCore;
using ShoppingMarket.Domain;

namespace ShoppingMarket.Infrastructure.Data;
#pragma warning disable CS1591
public class DataContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Product> Products { get; set; }
    public DataContext(DbContextOptions<DataContext> dbContext) : base(dbContext) { }
}
#pragma warning restore CS1591