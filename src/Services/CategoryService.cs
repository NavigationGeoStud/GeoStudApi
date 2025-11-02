using Microsoft.EntityFrameworkCore;
using GeoStud.Api.Data;
using GeoStud.Api.DTOs.Location;
using GeoStud.Api.Services.Interfaces;

namespace GeoStud.Api.Services;

public class CategoryService : ICategoryService
{
    private readonly GeoStudDbContext _context;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(GeoStudDbContext context, ILogger<CategoryService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<CategoryResponse>> GetCategoriesAsync()
    {
        var categories = await _context.LocationCategories
            .Where(c => !c.IsDeleted)
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

    public async Task<CategoryResponse?> GetCategoryByIdAsync(int id)
    {
        var category = await _context.LocationCategories
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

        if (category == null)
        {
            return null;
        }

        return new CategoryResponse
        {
            Id = category.Id,
            Name = category.Name,
            IconName = category.IconName,
            Description = category.Description,
            DisplayOrder = category.DisplayOrder,
            IsActive = category.IsActive,
            CreatedAt = category.CreatedAt
        };
    }

    public async Task<object> GetCategoryLocationsAsync(int id)
    {
        var category = await _context.LocationCategories
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

        if (category == null)
        {
            throw new ArgumentException("Category not found", nameof(id));
        }

        var locations = await _context.Locations
            .Where(l => l.CategoryId == id && !l.IsDeleted)
            .ToListAsync();

        var responses = locations.Select(l => new
        {
            l.Id,
            l.Name,
            l.Description,
            l.Coordinates,
            l.Address,
            l.City,
            l.Phone,
            l.Website,
            l.ImageUrl,
            l.Rating,
            l.RatingCount,
            l.PriceRange,
            l.WorkingHours,
            l.IsActive,
            l.IsVerified
        }).ToList();

        return new { 
            category = new {
                category.Id,
                category.Name,
                category.IconName,
                category.Description,
                category.DisplayOrder,
                category.IsActive,
                category.CreatedAt
            }, 
            locations = responses 
        };
    }

    public async Task<IEnumerable<SubcategoryResponse>> GetSubcategoriesAsync()
    {
        var subcategories = await _context.LocationSubcategories
            .Where(s => !s.IsDeleted)
            .OrderBy(s => s.DisplayOrder)
            .ToListAsync();

        return subcategories.Select(s => new SubcategoryResponse
        {
            Id = s.Id,
            Name = s.Name,
            CategoryId = s.CategoryId,
            Description = s.Description,
            DisplayOrder = s.DisplayOrder,
            IsActive = s.IsActive,
            CreatedAt = s.CreatedAt
        }).ToList();
    }

    public async Task<IEnumerable<SubcategoryResponse>> GetSubcategoriesByCategoryAsync(int categoryId)
    {
        var subcategories = await _context.LocationSubcategories
            .Where(s => s.CategoryId == categoryId && !s.IsDeleted)
            .OrderBy(s => s.DisplayOrder)
            .ToListAsync();

        return subcategories.Select(s => new SubcategoryResponse
        {
            Id = s.Id,
            Name = s.Name,
            CategoryId = s.CategoryId,
            Description = s.Description,
            DisplayOrder = s.DisplayOrder,
            IsActive = s.IsActive,
            CreatedAt = s.CreatedAt
        }).ToList();
    }

    public async Task<SubcategoryResponse?> GetSubcategoryByIdAsync(int id)
    {
        var subcategory = await _context.LocationSubcategories
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);

        if (subcategory == null)
        {
            return null;
        }

        return new SubcategoryResponse
        {
            Id = subcategory.Id,
            Name = subcategory.Name,
            CategoryId = subcategory.CategoryId,
            Description = subcategory.Description,
            DisplayOrder = subcategory.DisplayOrder,
            IsActive = subcategory.IsActive,
            CreatedAt = subcategory.CreatedAt
        };
    }
}

