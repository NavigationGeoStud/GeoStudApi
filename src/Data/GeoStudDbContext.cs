using Microsoft.EntityFrameworkCore;
using GeoStud.Api.Models;

namespace GeoStud.Api.Data;

public class GeoStudDbContext : DbContext
{
    public GeoStudDbContext(DbContextOptions<GeoStudDbContext> options) : base(options)
    {
        base.Database.EnsureCreated();
    }

    // Service entities
    public DbSet<ServiceClient> ServiceClients { get; set; }
    
    // GeoStud entities
    public DbSet<Student> Students { get; set; }
    public DbSet<StudentResponse> StudentResponses { get; set; }
    public DbSet<AnalyticsData> AnalyticsData { get; set; }

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

        // Global query filters for soft delete
        modelBuilder.Entity<ServiceClient>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Student>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<StudentResponse>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<AnalyticsData>().HasQueryFilter(e => !e.IsDeleted);
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
