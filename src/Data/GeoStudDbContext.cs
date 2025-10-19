using Microsoft.EntityFrameworkCore;
using GeoStud.Api.Models;

namespace GeoStud.Api.Data;

public class GeoStudDbContext : DbContext
{
    public GeoStudDbContext(DbContextOptions<GeoStudDbContext> options) : base(options)
    {
    }

    // Legacy entities (for backward compatibility)
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    public DbSet<ServiceClient> ServiceClients { get; set; }
    
    // GeoStud entities
    public DbSet<Student> Students { get; set; }
    public DbSet<StudentResponse> StudentResponses { get; set; }
    public DbSet<AnalyticsData> AnalyticsData { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        // Role configuration
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasIndex(e => e.Name).IsUnique();
        });

        // UserRole configuration
        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.RoleId });
            entity.HasOne(e => e.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(false); // Make navigation optional to avoid filter issues
            entity.HasOne(e => e.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(false); // Make navigation optional to avoid filter issues
        });

        // Permission configuration
        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasIndex(e => new { e.Resource, e.Action }).IsUnique();
        });

        // RolePermission configuration
        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.HasKey(e => new { e.RoleId, e.PermissionId });
            entity.HasOne(e => e.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(false); // Make navigation optional to avoid filter issues
            entity.HasOne(e => e.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(e => e.PermissionId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(false); // Make navigation optional to avoid filter issues
        });

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
        // Note: We'll apply filters selectively to avoid navigation issues
        modelBuilder.Entity<User>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ServiceClient>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Student>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<StudentResponse>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<AnalyticsData>().HasQueryFilter(e => !e.IsDeleted);
        
        // For Role and Permission, we'll handle soft delete in application logic
        // to avoid navigation property issues with UserRole and RolePermission
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
