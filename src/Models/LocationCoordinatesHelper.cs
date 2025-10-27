using System.Globalization;

namespace GeoStud.Api.Models;

public static class LocationCoordinatesHelper
{
    /// <summary>
    /// Parse coordinates from string format "latitude,longitude"
    /// </summary>
    public static (decimal? latitude, decimal? longitude) ParseCoordinates(string? coordinates)
    {
        if (string.IsNullOrWhiteSpace(coordinates))
            return (null, null);

        var parts = coordinates.Split(',');
        if (parts.Length != 2)
            return (null, null);

        if (decimal.TryParse(parts[0].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var lat) &&
            decimal.TryParse(parts[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var lng))
        {
            return (lat, lng);
        }

        return (null, null);
    }

    /// <summary>
    /// Format coordinates to string "latitude,longitude"
    /// </summary>
    public static string FormatCoordinates(decimal latitude, decimal longitude)
    {
        return $"{latitude.ToString(CultureInfo.InvariantCulture)},{longitude.ToString(CultureInfo.InvariantCulture)}";
    }

    /// <summary>
    /// Validate that the coordinates string is in correct format
    /// </summary>
    public static bool IsValidCoordinates(string? coordinates)
    {
        var (lat, lng) = ParseCoordinates(coordinates);
        return lat.HasValue && lng.HasValue;
    }
}

