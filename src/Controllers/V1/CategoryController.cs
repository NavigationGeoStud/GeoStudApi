using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GeoStud.Api.Data;
using GeoStud.Api.DTOs.Location;
using GeoStud.Api.Models;

namespace GeoStud.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class CategoryController : ControllerBase
{
    private readonly GeoStudDbContext _context;
    private readonly ILogger<CategoryController> _logger;

    public CategoryController(GeoStudDbContext context, ILogger<CategoryController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all categories
    /// </summary>
    /// <returns>List of categories</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CategoryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCategories()
    {
        try
        {
            var categories = await _context.LocationCategories
                .Where(c => !c.IsDeleted)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            var responses = categories.Select(c => new CategoryResponse
            {
                Id = c.Id,
                Name = c.Name,
                IconName = c.IconName,
                Description = c.Description,
                DisplayOrder = c.DisplayOrder,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt
            }).ToList();

            return Ok(responses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving categories");
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    /// Get category by ID
    /// </summary>
    /// <param name="id">Category ID</param>
    /// <returns>Category details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCategory(int id)
    {
        try
        {
            var category = await _context.LocationCategories
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

            if (category == null)
            {
                return NotFound();
            }

            var response = new CategoryResponse
            {
                Id = category.Id,
                Name = category.Name,
                IconName = category.IconName,
                Description = category.Description,
                DisplayOrder = category.DisplayOrder,
                IsActive = category.IsActive,
                CreatedAt = category.CreatedAt
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving category {Id}", id);
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    /// Get locations by category
    /// </summary>
    /// <param name="id">Category ID</param>
    /// <returns>List of locations in category</returns>
    [HttpGet("{id}/locations")]
    [ProducesResponseType(typeof(IEnumerable<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCategoryLocations(int id)
    {
        try
        {
            var category = await _context.LocationCategories
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

            if (category == null)
            {
                return NotFound();
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

            return Ok(new { 
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
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving locations for category {Id}", id);
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    /// Get all subcategories
    /// </summary>
    /// <returns>List of subcategories</returns>
    [HttpGet("subcategories")]
    [ProducesResponseType(typeof(IEnumerable<SubcategoryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetSubcategories()
    {
        try
        {
            var subcategories = await _context.LocationSubcategories
                .Where(s => !s.IsDeleted)
                .OrderBy(s => s.DisplayOrder)
                .ToListAsync();

            var responses = subcategories.Select(s => new SubcategoryResponse
            {
                Id = s.Id,
                Name = s.Name,
                CategoryId = s.CategoryId,
                Description = s.Description,
                DisplayOrder = s.DisplayOrder,
                IsActive = s.IsActive,
                CreatedAt = s.CreatedAt
            }).ToList();

            return Ok(responses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving subcategories");
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    /// Get subcategories by category ID
    /// </summary>
    /// <param name="categoryId">Category ID</param>
    /// <returns>List of subcategories in category</returns>
    [HttpGet("subcategories/by-category/{categoryId}")]
    [ProducesResponseType(typeof(IEnumerable<SubcategoryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetSubcategoriesByCategory(int categoryId)
    {
        try
        {
            var subcategories = await _context.LocationSubcategories
                .Where(s => s.CategoryId == categoryId && !s.IsDeleted)
                .OrderBy(s => s.DisplayOrder)
                .ToListAsync();

            var responses = subcategories.Select(s => new SubcategoryResponse
            {
                Id = s.Id,
                Name = s.Name,
                CategoryId = s.CategoryId,
                Description = s.Description,
                DisplayOrder = s.DisplayOrder,
                IsActive = s.IsActive,
                CreatedAt = s.CreatedAt
            }).ToList();

            return Ok(responses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving subcategories for category {CategoryId}", categoryId);
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    /// Get subcategory by ID
    /// </summary>
    /// <param name="id">Subcategory ID</param>
    /// <returns>Subcategory details</returns>
    [HttpGet("subcategories/{id}")]
    [ProducesResponseType(typeof(SubcategoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetSubcategory(int id)
    {
        try
        {
            var subcategory = await _context.LocationSubcategories
                .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);

            if (subcategory == null)
            {
                return NotFound();
            }

            var response = new SubcategoryResponse
            {
                Id = subcategory.Id,
                Name = subcategory.Name,
                CategoryId = subcategory.CategoryId,
                Description = subcategory.Description,
                DisplayOrder = subcategory.DisplayOrder,
                IsActive = subcategory.IsActive,
                CreatedAt = subcategory.CreatedAt
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving subcategory {Id}", id);
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }
}

