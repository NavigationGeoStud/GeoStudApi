using Microsoft.EntityFrameworkCore;
using GeoStud.Api.Models;

namespace GeoStud.Api.Data;

public static class SeedData
{
    public static async Task SeedAsync(GeoStudDbContext context)
    {
        if (!context.Roles.Any())
        {
            var roles = new List<Role>
            {
                new Role { Name = "Admin", Description = "System Administrator" },
                new Role { Name = "User", Description = "Regular User" },
                new Role { Name = "Service", Description = "Service Account" }
            };

            context.Roles.AddRange(roles);
            await context.SaveChangesAsync();
        }

        if (!context.Permissions.Any())
        {
            var permissions = new List<Permission>
            {
                new Permission { Name = "ReadGeoLocations", Description = "Read geo locations", Resource = "GeoLocation", Action = "Read" },
                new Permission { Name = "WriteGeoLocations", Description = "Create/Update geo locations", Resource = "GeoLocation", Action = "Write" },
                new Permission { Name = "DeleteGeoLocations", Description = "Delete geo locations", Resource = "GeoLocation", Action = "Delete" },
                new Permission { Name = "ManageUsers", Description = "Manage users", Resource = "User", Action = "Manage" },
                new Permission { Name = "ManageRoles", Description = "Manage roles", Resource = "Role", Action = "Manage" }
            };

            context.Permissions.AddRange(permissions);
            await context.SaveChangesAsync();
        }

        if (!context.Users.Any())
        {
            var adminRole = await context.Roles.FirstAsync(r => r.Name == "Admin");
            var userRole = await context.Roles.FirstAsync(r => r.Name == "User");

            var adminUser = new User
            {
                Username = "admin",
                Email = "admin@geostud.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                FirstName = "Admin",
                LastName = "User",
                IsActive = true
            };

            context.Users.Add(adminUser);
            await context.SaveChangesAsync();

            // Assign admin role
            var adminUserRole = new UserRole
            {
                UserId = adminUser.Id,
                RoleId = adminRole.Id
            };

            context.UserRoles.Add(adminUserRole);
            await context.SaveChangesAsync();
        }

        if (!context.ServiceClients.Any())
        {
            var serviceClients = new List<ServiceClient>
            {
                new ServiceClient
                {
                    ClientId = "mobile-app",
                    ClientSecret = BCrypt.Net.BCrypt.HashPassword("MobileAppSecret123!"),
                    ServiceName = "Mobile Application",
                    Description = "Mobile application service client",
                    IsActive = true,
                    AllowedScopes = "read:locations,write:locations"
                },
                new ServiceClient
                {
                    ClientId = "web-app",
                    ClientSecret = BCrypt.Net.BCrypt.HashPassword("WebAppSecret123!"),
                    ServiceName = "Web Application",
                    Description = "Web application service client",
                    IsActive = true,
                    AllowedScopes = "read:locations,write:locations,delete:locations"
                }
            };

            context.ServiceClients.AddRange(serviceClients);
            await context.SaveChangesAsync();
        }

        // Assign permissions to roles
        if (!context.RolePermissions.Any())
        {
            var adminRole = await context.Roles.FirstAsync(r => r.Name == "Admin");
            var userRole = await context.Roles.FirstAsync(r => r.Name == "User");

            var allPermissions = await context.Permissions.ToListAsync();
            var userPermissions = allPermissions.Where(p => p.Name.Contains("GeoLocation") || p.Name.Contains("Survey")).ToList();

            // Admin gets all permissions
            foreach (var permission in allPermissions)
            {
                context.RolePermissions.Add(new RolePermission
                {
                    RoleId = adminRole.Id,
                    PermissionId = permission.Id
                });
            }

            // User gets survey and geo location permissions
            foreach (var permission in userPermissions)
            {
                context.RolePermissions.Add(new RolePermission
                {
                    RoleId = userRole.Id,
                    PermissionId = permission.Id
                });
            }

            await context.SaveChangesAsync();
        }

        // Add survey permissions
        if (!context.Permissions.Any(p => p.Name.Contains("Survey")))
        {
            var surveyPermissions = new List<Permission>
            {
                new Permission { Name = "ReadSurveys", Description = "Read survey data", Resource = "Survey", Action = "Read" },
                new Permission { Name = "WriteSurveys", Description = "Submit survey data", Resource = "Survey", Action = "Write" },
                new Permission { Name = "ReadAnalytics", Description = "Read analytics data", Resource = "Analytics", Action = "Read" }
            };

            context.Permissions.AddRange(surveyPermissions);
            await context.SaveChangesAsync();
        }
    }
}
