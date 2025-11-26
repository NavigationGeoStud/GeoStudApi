using System.Collections.Generic;

namespace GeoStud.Api.Helpers;

/// <summary>
/// Helper class for expanding interest categories to their subcategories
/// </summary>
public static class InterestCategoryHelper
{
    /// <summary>
    /// Dictionary mapping main categories to their subcategories
    /// </summary>
    private static readonly Dictionary<string, List<string>> CategorySubcategories = new(StringComparer.OrdinalIgnoreCase)
    {
        ["theatre"] = new List<string>
        {
            "theatre:drama",
            "theatre:comedy",
            "theatre:musical",
            "theatre:kids",
            "theatre:art",
            "theatre:classic"
        },
        ["movie"] = new List<string>
        {
            "movie:drama",
            "movie:comedy",
            "movie:action",
            "movie:horror",
            "movie:sci-fi",
            "movie:romance",
            "movie:detective",
            "movie:arthouse"
        },
        ["concerts"] = new List<string>
        {
            "concerts:rok",
            "concerts:pop",
            "concerts:jazz",
            "concerts:electronic",
            "concerts:classical",
            "concerts:indi",
            "concerts:altenative"
        },
        ["clubs"] = new List<string>
        {
            // Clubs category doesn't have subcategories yet, but we keep it for future extensibility
            // If subcategories are added later, they should be added here
        }
    };

    /// <summary>
    /// Valid interest categories (main categories without subcategories)
    /// </summary>
    public static readonly HashSet<string> ValidCategories = new(StringComparer.OrdinalIgnoreCase)
    {
        "theatre",
        "movie",
        "concerts",
        "clubs",
        "museums",
        "landmark",
        "suburban",
        "tourist"
    };

    /// <summary>
    /// Expands a list of interest categories to include all their subcategories.
    /// If a category has no subcategories, it remains as is.
    /// If a subcategory is already present (old format), it remains as is for backward compatibility.
    /// </summary>
    /// <param name="interests">List of interest categories (e.g., ["theatre", "movie"])</param>
    /// <returns>Expanded list with all subcategories (e.g., ["theatre", "theatre:drama", "theatre:comedy", ..., "movie", "movie:drama", ...])</returns>
    public static List<string> ExpandCategoriesToSubcategories(List<string> interests)
    {
        if (interests == null || interests.Count == 0)
        {
            return new List<string>();
        }

        var expandedInterests = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var interest in interests)
        {
            if (string.IsNullOrWhiteSpace(interest))
                continue;

            // Check if it's already a subcategory (contains colon)
            if (interest.Contains(':'))
            {
                // It's already a subcategory - keep it for backward compatibility
                expandedInterests.Add(interest);
            }
            else
            {
                // It's a main category - add it and all its subcategories
                var category = interest.Trim().ToLowerInvariant();
                
                // Add the main category itself
                expandedInterests.Add(category);

                // Add all subcategories if they exist
                if (CategorySubcategories.TryGetValue(category, out var subcategories) && subcategories.Count > 0)
                {
                    foreach (var subcategory in subcategories)
                    {
                        expandedInterests.Add(subcategory);
                    }
                }
            }
        }

        return expandedInterests.ToList();
    }

    /// <summary>
    /// Validates if a category is valid
    /// </summary>
    /// <param name="category">Category name to validate</param>
    /// <returns>True if valid, false otherwise</returns>
    public static bool IsValidCategory(string category)
    {
        if (string.IsNullOrWhiteSpace(category))
            return false;

        // Check if it's a main category
        if (ValidCategories.Contains(category))
            return true;

        // Check if it's a valid subcategory (format: "category:subcategory")
        var parts = category.Split(':', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 2)
        {
            var mainCategory = parts[0].Trim().ToLowerInvariant();
            if (CategorySubcategories.TryGetValue(mainCategory, out var subcategories))
            {
                return subcategories.Contains(category, StringComparer.OrdinalIgnoreCase);
            }
        }

        return false;
    }

    /// <summary>
    /// Validates a list of interests
    /// </summary>
    /// <param name="interests">List of interests to validate</param>
    /// <returns>True if all interests are valid, false otherwise</returns>
    public static bool ValidateInterests(List<string> interests)
    {
        if (interests == null || interests.Count == 0)
            return true; // Empty list is valid

        return interests.All(IsValidCategory);
    }
}

