using Microsoft.EntityFrameworkCore;
using GeoStud.Api.Models;

namespace GeoStud.Api.Data;

public class GeoStudDbContext : DbContext
{
    public GeoStudDbContext(DbContextOptions<GeoStudDbContext> options) : base(options)
    {
    }

    // Service entities
    public DbSet<ServiceClient> ServiceClients { get; set; }
    
    // GeoStud entities
    public DbSet<Student> Students { get; set; }
    public DbSet<StudentResponse> StudentResponses { get; set; }
    public DbSet<AnalyticsData> AnalyticsData { get; set; }
    
    // Location entities
    public DbSet<Location> Locations { get; set; }
    public DbSet<LocationCategory> LocationCategories { get; set; }
    public DbSet<LocationSubcategory> LocationSubcategories { get; set; }
    public DbSet<LocationCategoryJoin> LocationCategoryJoins { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);


        // ServiceClient configuration
        modelBuilder.Entity<ServiceClient>(entity =>
        {
            entity.HasIndex(e => e.ClientId).IsUnique();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        // Student configuration
        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.TelegramId).IsUnique();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasIndex(e => e.AgeRange);
            entity.HasIndex(e => e.Gender);
            entity.HasIndex(e => e.IsStudent);
            entity.HasIndex(e => e.IsLocal);
        });

        // StudentResponse configuration
        modelBuilder.Entity<StudentResponse>(entity =>
        {
            entity.HasIndex(e => new { e.StudentId, e.Question });
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasOne(e => e.Student)
                .WithMany(s => s.Responses)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // AnalyticsData configuration
        modelBuilder.Entity<AnalyticsData>(entity =>
        {
            entity.HasIndex(e => new { e.MetricName, e.Category });
            entity.HasIndex(e => e.CalculatedAt);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        // Location configuration
        modelBuilder.Entity<Location>(entity =>
        {
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.Coordinates);
            entity.HasIndex(e => e.City);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.IsVerified);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        // LocationCategory configuration
        modelBuilder.Entity<LocationCategory>(entity =>
        {
            entity.HasIndex(e => e.Name).IsUnique();
            entity.HasIndex(e => e.DisplayOrder);
            entity.HasIndex(e => e.IsActive);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        // LocationSubcategory configuration
        modelBuilder.Entity<LocationSubcategory>(entity =>
        {
            entity.HasIndex(e => new { e.CategoryId, e.Name }).IsUnique();
            entity.HasIndex(e => e.DisplayOrder);
            entity.HasIndex(e => e.IsActive);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasOne(e => e.Category)
                .WithMany(c => c.Subcategories)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // LocationCategoryJoin configuration (many-to-many)
        modelBuilder.Entity<LocationCategoryJoin>(entity =>
        {
            entity.HasKey(e => new { e.LocationId, e.CategoryId });
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasOne(e => e.Location)
                .WithMany(l => l.CategoryJoins)
                .HasForeignKey(e => e.LocationId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Category)
                .WithMany(c => c.LocationJoins)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Global query filters for soft delete
        modelBuilder.Entity<ServiceClient>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Student>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<StudentResponse>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<AnalyticsData>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Location>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<LocationCategory>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<LocationSubcategory>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<LocationCategoryJoin>().HasQueryFilter(e => !e.IsDeleted);
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries<BaseEntity>()
            .Where(e => e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            entry.Entity.UpdatedAt = DateTime.UtcNow;
        }
    }
}
