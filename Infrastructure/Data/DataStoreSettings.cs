namespace ShoppingMarket.Infrastructure.Data;

public class DataStoreSettings
{
    public string ConnectionString { get; set; } = null!;
    public string DatabaseName { get; set; } = null!;
    public string UsersCollectionName { get; set; } = null!;
    public string ProductsCollectionName { get; set; } = null!;
}