using GeoStud.Api.DTOs.Location;

namespace GeoStud.Api.Services.Interfaces;

public interface ICategoryService
{
    Task<IEnumerable<CategoryResponse>> GetCategoriesAsync();
    Task<CategoryResponse?> GetCategoryByIdAsync(int id);
    Task<object> GetCategoryLocationsAsync(int id);
    Task<IEnumerable<SubcategoryResponse>> GetSubcategoriesAsync();
    Task<IEnumerable<SubcategoryResponse>> GetSubcategoriesByCategoryAsync(int categoryId);
    Task<SubcategoryResponse?> GetSubcategoryByIdAsync(int id);
}

