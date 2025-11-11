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
    public DbSet<User> Users { get; set; }
    public DbSet<UserAnalyticsResponse> UserAnalyticsResponses { get; set; }
    public DbSet<AnalyticsData> AnalyticsData { get; set; }
    public DbSet<FavoriteLocation> FavoriteLocations { get; set; }
    public DbSet<UserLike> UserLikes { get; set; }
    public DbSet<UserDislike> UserDislikes { get; set; }
    public DbSet<Match> Matches { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<LocationSuggestion> LocationSuggestions { get; set; }
    
    // Location entities
    public DbSet<Location> Locations { get; set; }
    public DbSet<LocationCategory> LocationCategories { get; set; }
    public DbSet<LocationSubcategory> LocationSubcategories { get; set; }
    public DbSet<LocationSubcategoryJoin> LocationSubcategoryJoins { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);


        // ServiceClient configuration
        modelBuilder.Entity<ServiceClient>(entity =>
        {
            entity.HasIndex(e => e.ClientId).IsUnique();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Username).IsUnique();
            // Email can be nullable for Telegram users, but if provided must be unique
            // SQLite allows null values in unique indexes, multiple nulls are allowed
            entity.HasIndex(e => e.Email).IsUnique();
            // TelegramId must be unique if provided
            entity.HasIndex(e => e.TelegramId).IsUnique();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasIndex(e => e.AgeRange);
            entity.HasIndex(e => e.Gender);
            entity.HasIndex(e => e.IsStudent);
            entity.HasIndex(e => e.IsLocal);
            // Role configuration - store as integer
            // Default value is set in the model (UserRole.User = 0)
            entity.Property(e => e.Role)
                .HasConversion<int>();
            entity.HasIndex(e => e.Role);
        });

        // UserAnalyticsResponse configuration
        modelBuilder.Entity<UserAnalyticsResponse>(entity =>
        {
            entity.HasIndex(e => new { e.UserId, e.Question });
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasOne(e => e.User)
                .WithMany(u => u.Responses)
                .HasForeignKey(e => e.UserId)
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
            entity.HasIndex(e => e.CategoryId);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        // LocationCategory configuration
        modelBuilder.Entity<LocationCategory>(entity =>
        {
            entity.HasIndex(e => e.Name).IsUnique();
            entity.HasIndex(e => e.DisplayOrder);
            entity.HasIndex(e => e.IsActive);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasMany(e => e.Locations)
                .WithOne(l => l.Category)
                .HasForeignKey(l => l.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
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

        // LocationSubcategoryJoin configuration (many-to-many)
        modelBuilder.Entity<LocationSubcategoryJoin>(entity =>
        {
            entity.HasKey(e => new { e.LocationId, e.SubcategoryId });
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasOne(e => e.Location)
                .WithMany(l => l.SubcategoryJoins)
                .HasForeignKey(e => e.LocationId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Subcategory)
                .WithMany(s => s.LocationJoins)
                .HasForeignKey(e => e.SubcategoryId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // FavoriteLocation configuration
        modelBuilder.Entity<FavoriteLocation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserId, e.LocationId }).IsUnique();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasOne(e => e.User)
                .WithMany(u => u.FavoriteLocations)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Location)
                .WithMany(l => l.FavoriteLocations)
                .HasForeignKey(e => e.LocationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // UserLike configuration
        modelBuilder.Entity<UserLike>(entity =>
        {
            entity.HasIndex(e => new { e.UserId, e.TargetUserId }).IsUnique();
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.TargetUserId);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.TargetUser)
                .WithMany()
                .HasForeignKey(e => e.TargetUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // UserDislike configuration
        modelBuilder.Entity<UserDislike>(entity =>
        {
            entity.HasIndex(e => new { e.UserId, e.TargetUserId }).IsUnique();
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.TargetUserId);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.TargetUser)
                .WithMany()
                .HasForeignKey(e => e.TargetUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Match configuration
        modelBuilder.Entity<Match>(entity =>
        {
            entity.HasIndex(e => new { e.UserId1, e.UserId2 }).IsUnique();
            entity.HasIndex(e => e.UserId1);
            entity.HasIndex(e => e.UserId2);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasOne(e => e.User1)
                .WithMany()
                .HasForeignKey(e => e.UserId1)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.User2)
                .WithMany()
                .HasForeignKey(e => e.UserId2)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Notification configuration
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasIndex(e => e.TelegramId);
            entity.HasIndex(e => new { e.TelegramId, e.IsRead });
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.Type);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasOne(e => e.Location)
                .WithMany()
                .HasForeignKey(e => e.LocationId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // LocationSuggestion configuration
        modelBuilder.Entity<LocationSuggestion>(entity =>
        {
            entity.HasIndex(e => new { e.TelegramId, e.LocationId }).IsUnique();
            entity.HasIndex(e => e.TelegramId);
            entity.HasIndex(e => e.LocationId);
            entity.HasIndex(e => e.Status);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasOne(e => e.Location)
                .WithMany()
                .HasForeignKey(e => e.LocationId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Global query filters for soft delete
        modelBuilder.Entity<ServiceClient>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<User>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<UserAnalyticsResponse>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<AnalyticsData>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<FavoriteLocation>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<UserLike>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<UserDislike>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Match>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Notification>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<LocationSuggestion>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Location>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<LocationCategory>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<LocationSubcategory>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<LocationSubcategoryJoin>().HasQueryFilter(e => !e.IsDeleted);
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
