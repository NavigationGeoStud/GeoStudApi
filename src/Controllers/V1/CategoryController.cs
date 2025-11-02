using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GeoStud.Api.DTOs.Location;
using GeoStud.Api.Services.Interfaces;

namespace GeoStud.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]

[Authorize]
[Tags("Private")]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    private readonly ILogger<CategoryController> _logger;

    public CategoryController(ICategoryService categoryService, ILogger<CategoryController> logger)
    {
        _categoryService = categoryService;
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
            var categories = await _categoryService.GetCategoriesAsync();
            return Ok(categories);
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
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return Ok(category);
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
            var result = await _categoryService.GetCategoryLocationsAsync(id);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
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
            var subcategories = await _categoryService.GetSubcategoriesAsync();
            return Ok(subcategories);
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
            var subcategories = await _categoryService.GetSubcategoriesByCategoryAsync(categoryId);
            return Ok(subcategories);
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
            var subcategory = await _categoryService.GetSubcategoryByIdAsync(id);
            if (subcategory == null)
            {
                return NotFound();
            }
            return Ok(subcategory);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving subcategory {Id}", id);
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }
}

