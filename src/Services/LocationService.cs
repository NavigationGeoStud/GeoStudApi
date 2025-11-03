using Microsoft.EntityFrameworkCore;
using GeoStud.Api.Data;
using GeoStud.Api.DTOs.Location;
using GeoStud.Api.Models;
using GeoStud.Api.Services.Interfaces;
using CreateLocationTelegramRequest = GeoStud.Api.DTOs.Location.CreateLocationTelegramRequest;

namespace GeoStud.Api.Services;

public class LocationService : ILocationService
{
    private readonly GeoStudDbContext _context;
    private readonly ILogger<LocationService> _logger;

    public LocationService(GeoStudDbContext context, ILogger<LocationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<LocationResponse>> GetLocationsAsync(int? categoryId = null, int? subcategoryId = null, string? city = null)
    {
        var query = _context.Locations.AsQueryable();

        // Filter by category if provided
        if (categoryId.HasValue)
        {
            query = query.Where(l => l.CategoryId == categoryId.Value);
        }

        // Filter by subcategory if provided
        if (subcategoryId.HasValue)
        {
            query = query.Where(l => l.SubcategoryJoins.Any(sj => sj.SubcategoryId == subcategoryId.Value));
        }

        // Filter by city if provided
        if (!string.IsNullOrWhiteSpace(city))
        {
            query = query.Where(l => l.City == city);
        }

        var locations = await query
            .Include(l => l.Category)
            .Include(l => l.SubcategoryJoins)
                .ThenInclude(sj => sj.Subcategory)
            .ToListAsync();

        return locations.Select(ToLocationResponse).ToList();
    }

    public async Task<LocationResponse?> GetLocationByIdAsync(int id)
    {
        var location = await _context.Locations
            .Include(l => l.Category)
            .Include(l => l.SubcategoryJoins)
                .ThenInclude(sj => sj.Subcategory)
            .FirstOrDefaultAsync(l => l.Id == id && !l.IsDeleted);

        if (location == null)
        {
            return null;
        }

        return ToLocationResponse(location);
    }

    public async Task<IEnumerable<LocationResponse>> GetLocationsByCategoryAsync(int categoryId)
    {
        // Check if category exists
        var category = await _context.LocationCategories
            .FirstOrDefaultAsync(c => c.Id == categoryId && !c.IsDeleted);

        if (category == null)
        {
            throw new ArgumentException("Category not found", nameof(categoryId));
        }

        var locations = await _context.Locations
            .Where(l => l.CategoryId == categoryId && !l.IsDeleted)
            .Include(l => l.Category)
            .Include(l => l.SubcategoryJoins)
                .ThenInclude(sj => sj.Subcategory)
            .ToListAsync();

        return locations.Select(ToLocationResponse).ToList();
    }

    public async Task<LocationResponse> CreateLocationAsync(LocationRequest request)
    {
        // Validate coordinates format
        if (!LocationCoordinatesHelper.IsValidCoordinates(request.Coordinates))
        {
            throw new ArgumentException("Invalid coordinates format. Expected format: 'latitude,longitude'", nameof(request));
        }

        // Validate category exists and is active
        var category = await _context.LocationCategories
            .FirstOrDefaultAsync(c => c.Id == request.CategoryId && c.IsActive && !c.IsDeleted);

        if (category == null)
        {
            throw new ArgumentException("Category not found or inactive", nameof(request));
        }

        // Validate subcategories belong to the category
        if (request.SubcategoryIds.Any())
        {
            var invalidSubcategories = await _context.LocationSubcategories
                .Where(s => request.SubcategoryIds.Contains(s.Id) && (s.CategoryId != request.CategoryId || !s.IsActive))
                .ToListAsync();

            if (invalidSubcategories.Any())
            {
                throw new ArgumentException("Some subcategories do not belong to the selected category or are inactive", nameof(request));
            }
        }

        var location = new Location
        {
            Name = request.Name,
            Description = request.Description,
            Coordinates = request.Coordinates,
            Address = request.Address,
            City = request.City,
            Phone = request.Phone,
            Website = request.Website,
            TelegramImageIds = request.TelegramImageIds,
            Rating = request.Rating,
            RatingCount = request.RatingCount,
            PriceRange = request.PriceRange,
            WorkingHours = request.WorkingHours,
            IsActive = request.IsActive,
            IsVerified = request.IsVerified,
            CategoryId = request.CategoryId
        };

        _context.Locations.Add(location);
        await _context.SaveChangesAsync();

        // Add subcategory associations
        if (request.SubcategoryIds.Any())
        {
            var validSubcategories = await _context.LocationSubcategories
                .Where(s => request.SubcategoryIds.Contains(s.Id) && s.IsActive)
                .ToListAsync();

            var subcategoryJoins = validSubcategories.Select(s => new LocationSubcategoryJoin
            {
                LocationId = location.Id,
                SubcategoryId = s.Id
            }).ToList();

            _context.LocationSubcategoryJoins.AddRange(subcategoryJoins);
            await _context.SaveChangesAsync();
        }

        // Reload with category and subcategories
        await _context.Entry(location)
            .Reference(l => l.Category)
            .LoadAsync();
            
        await _context.Entry(location)
            .Collection(l => l.SubcategoryJoins)
            .Query()
            .Include(sj => sj.Subcategory)
            .LoadAsync();

        return ToLocationResponse(location);
    }

    public async Task<LocationResponse> UpdateLocationAsync(int id, LocationRequest request)
    {
        // Validate coordinates format
        if (!LocationCoordinatesHelper.IsValidCoordinates(request.Coordinates))
        {
            throw new ArgumentException("Invalid coordinates format. Expected format: 'latitude,longitude'", nameof(request));
        }

        // Validate category exists and is active
        var category = await _context.LocationCategories
            .FirstOrDefaultAsync(c => c.Id == request.CategoryId && c.IsActive && !c.IsDeleted);

        if (category == null)
        {
            throw new ArgumentException("Category not found or inactive", nameof(request));
        }

        var location = await _context.Locations
            .Include(l => l.SubcategoryJoins)
            .FirstOrDefaultAsync(l => l.Id == id && !l.IsDeleted);

        if (location == null)
        {
            throw new ArgumentException("Location not found", nameof(id));
        }

        // Validate subcategories belong to the category
        if (request.SubcategoryIds.Any())
        {
            var invalidSubcategories = await _context.LocationSubcategories
                .Where(s => request.SubcategoryIds.Contains(s.Id) && (s.CategoryId != request.CategoryId || !s.IsActive))
                .ToListAsync();

            if (invalidSubcategories.Any())
            {
                throw new ArgumentException("Some subcategories do not belong to the selected category or are inactive", nameof(request));
            }
        }

        // Update location properties
        location.Name = request.Name;
        location.Description = request.Description;
        location.Coordinates = request.Coordinates;
        location.Address = request.Address;
        location.City = request.City;
        location.Phone = request.Phone;
        location.Website = request.Website;
        location.TelegramImageIds = request.TelegramImageIds;
        location.Rating = request.Rating;
        location.RatingCount = request.RatingCount;
        location.PriceRange = request.PriceRange;
        location.WorkingHours = request.WorkingHours;
        location.IsActive = request.IsActive;
        location.IsVerified = request.IsVerified;
        location.CategoryId = request.CategoryId;

        // Update subcategory associations
        var existingSubcategoryIds = location.SubcategoryJoins.Select(sj => sj.SubcategoryId).ToList();
        var newSubcategoryIds = request.SubcategoryIds.ToList();

        // Remove old subcategory associations
        var subcategoriesToRemove = location.SubcategoryJoins
            .Where(sj => !newSubcategoryIds.Contains(sj.SubcategoryId))
            .ToList();
        foreach (var join in subcategoriesToRemove)
        {
            _context.LocationSubcategoryJoins.Remove(join);
        }

        // Add new subcategory associations
        var subcategoriesToAdd = newSubcategoryIds
            .Where(sid => !existingSubcategoryIds.Contains(sid))
            .ToList();

        if (subcategoriesToAdd.Any())
        {
            var validSubcategories = await _context.LocationSubcategories
                .Where(s => subcategoriesToAdd.Contains(s.Id) && s.IsActive)
                .ToListAsync();

            var newSubcategoryJoins = validSubcategories.Select(s => new LocationSubcategoryJoin
            {
                LocationId = location.Id,
                SubcategoryId = s.Id
            }).ToList();

            _context.LocationSubcategoryJoins.AddRange(newSubcategoryJoins);
        }

        await _context.SaveChangesAsync();

        // Reload with category and subcategories
        await _context.Entry(location)
            .Reference(l => l.Category)
            .LoadAsync();
            
        await _context.Entry(location)
            .Collection(l => l.SubcategoryJoins)
            .Query()
            .Include(sj => sj.Subcategory)
            .LoadAsync();

        return ToLocationResponse(location);
    }

    public async Task<bool> DeleteLocationAsync(int id)
    {
        var location = await _context.Locations
            .FirstOrDefaultAsync(l => l.Id == id && !l.IsDeleted);

        if (location == null)
        {
            return false;
        }

        location.IsDeleted = true;
        location.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<IEnumerable<CategoryResponse>> GetCategoriesAsync()
    {
        var categories = await _context.LocationCategories
            .Where(c => c.IsActive && !c.IsDeleted)
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync();

        return categories.Select(c => new CategoryResponse
        {
            Id = c.Id,
            Name = c.Name,
            IconName = c.IconName,
            Description = c.Description,
            DisplayOrder = c.DisplayOrder,
            IsActive = c.IsActive,
            CreatedAt = c.CreatedAt
        }).ToList();
    }

    public async Task<IEnumerable<LocationResponse>> GetNearbyLocationsAsync(string coordinates, double radiusKm = 5)
    {
        var (lat, lng) = LocationCoordinatesHelper.ParseCoordinates(coordinates);
        if (!lat.HasValue || !lng.HasValue)
        {
            throw new ArgumentException("Invalid coordinates format. Expected format: 'latitude,longitude'", nameof(coordinates));
        }

        // Note: This is a simplified implementation
        // For production, consider using SQL spatial queries or a proper geospatial library
        var locations = await _context.Locations
            .Include(l => l.Category)
            .Include(l => l.SubcategoryJoins)
                .ThenInclude(sj => sj.Subcategory)
            .ToListAsync();

        var nearby = locations.Where(l =>
        {
            var (locLat, locLng) = LocationCoordinatesHelper.ParseCoordinates(l.Coordinates);
            if (!locLat.HasValue || !locLng.HasValue)
                return false;

            var distance = CalculateDistance(
                lat.Value, lng.Value,
                locLat.Value, locLng.Value
            );

            return distance <= radiusKm;
        }).ToList();

        return nearby.Select(ToLocationResponse).ToList();
    }

    private static LocationResponse ToLocationResponse(Location location)
    {
        return new LocationResponse
        {
            Id = location.Id,
            Name = location.Name,
            Description = location.Description,
            Coordinates = location.Coordinates,
            Address = location.Address,
            City = location.City,
            Phone = location.Phone,
            Website = location.Website,
            TelegramImageIds = location.TelegramImageIds,
            Rating = location.Rating,
            RatingCount = location.RatingCount,
            PriceRange = location.PriceRange,
            WorkingHours = location.WorkingHours,
            IsActive = location.IsActive,
            IsVerified = location.IsVerified,
            CreatedAt = location.CreatedAt,
            UpdatedAt = location.UpdatedAt,
            Category = new LocationResponse.CategoryInfo
            {
                Id = location.Category.Id,
                Name = location.Category.Name,
                IconName = location.Category.IconName
            },
            Subcategories = location.SubcategoryJoins
                .Select(sj => new LocationResponse.SubcategoryInfo
                {
                    Id = sj.Subcategory.Id,
                    Name = sj.Subcategory.Name,
                    CategoryId = sj.Subcategory.CategoryId
                })
                .ToList()
        };
    }

    private static double CalculateDistance(decimal lat1, decimal lng1, decimal lat2, decimal lng2)
    {
        // Haversine formula for calculating distance between two points
        const double earthRadiusKm = 6371.0;

        var dLat = ToRadians((double)(lat2 - lat1));
        var dLng = ToRadians((double)(lng2 - lng1));

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians((double)lat1)) * Math.Cos(ToRadians((double)lat2)) *
                Math.Sin(dLng / 2) * Math.Sin(dLng / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return earthRadiusKm * c;
    }

    private static double ToRadians(double degrees)
    {
        return degrees * Math.PI / 180.0;
    }

    public async Task<LocationResponse> CreateLocationFromTelegramAsync(CreateLocationTelegramRequest request)
    {
        // Convert to LocationRequest format for validation
        var locationRequest = new LocationRequest
        {
            CategoryId = request.CategoryId,
            SubcategoryIds = request.SubcategoryIds ?? new List<int>(),
            Name = request.Name,
            Description = request.Description,
            Coordinates = request.Coordinates,
            Address = request.Address,
            City = request.City,
            Phone = request.Phone,
            Website = request.Website,
            Rating = request.Rating,
            PriceRange = request.PriceRange,
            WorkingHours = request.WorkingHours,
            IsActive = true,
            IsVerified = false
        };

        // Validate coordinates format
        if (!LocationCoordinatesHelper.IsValidCoordinates(request.Coordinates))
        {
            throw new ArgumentException("Invalid coordinates format. Expected format: 'latitude,longitude'", nameof(request));
        }

        // Validate category exists and is active
        var category = await _context.LocationCategories
            .FirstOrDefaultAsync(c => c.Id == request.CategoryId && c.IsActive && !c.IsDeleted);

        if (category == null)
        {
            throw new ArgumentException("Category not found or inactive", nameof(request));
        }

        // Validate subcategories belong to the category
        if (locationRequest.SubcategoryIds.Any())
        {
            var invalidSubcategories = await _context.LocationSubcategories
                .Where(s => locationRequest.SubcategoryIds.Contains(s.Id) && (s.CategoryId != request.CategoryId || !s.IsActive))
                .ToListAsync();

            if (invalidSubcategories.Any())
            {
                throw new ArgumentException("Some subcategories do not belong to the selected category or are inactive", nameof(request));
            }
        }

        var location = new Location
        {
            Name = request.Name,
            Description = request.Description,
            Coordinates = request.Coordinates,
            Address = request.Address,
            City = request.City,
            Phone = request.Phone,
            Website = request.Website,
            TelegramImageIds = request.TelegramImageIds,
            Rating = request.Rating,
            PriceRange = request.PriceRange,
            WorkingHours = request.WorkingHours,
            IsActive = true,
            IsVerified = false,
            CategoryId = request.CategoryId
        };

        _context.Locations.Add(location);
        await _context.SaveChangesAsync();

        // Add subcategory associations
        if (locationRequest.SubcategoryIds.Any())
        {
            var validSubcategories = await _context.LocationSubcategories
                .Where(s => locationRequest.SubcategoryIds.Contains(s.Id) && s.IsActive)
                .ToListAsync();

            var subcategoryJoins = validSubcategories.Select(s => new LocationSubcategoryJoin
            {
                LocationId = location.Id,
                SubcategoryId = s.Id
            }).ToList();

            _context.LocationSubcategoryJoins.AddRange(subcategoryJoins);
            await _context.SaveChangesAsync();
        }

        // Reload with category and subcategories
        await _context.Entry(location)
            .Reference(l => l.Category)
            .LoadAsync();
            
        await _context.Entry(location)
            .Collection(l => l.SubcategoryJoins)
            .Query()
            .Include(sj => sj.Subcategory)
            .LoadAsync();

        return ToLocationResponse(location);
    }
}

