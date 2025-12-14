using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GeoStud.Api.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<GeoStudDbContext>
{
    public GeoStudDbContext CreateDbContext(string[] args)
    {
        // For design-time operations (migrations), use a default connection string
        // This will be overridden at runtime by the actual connection string from configuration
        var optionsBuilder = new DbContextOptionsBuilder<GeoStudDbContext>();
        
        // Use a default PostgreSQL connection string for design-time
        // This doesn't need to be a real database - EF Core just needs to know the provider
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection") 
            ?? "Host=localhost;Port=5432;Database=GeoStudDb;Username=postgres;Password=postgres";
        
        optionsBuilder.UseNpgsql(connectionString);
        
        return new GeoStudDbContext(optionsBuilder.Options);
    }
}

