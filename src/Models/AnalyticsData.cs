using System.ComponentModel.DataAnnotations;

namespace GeoStud.Api.Models;

public class AnalyticsData : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string MetricName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string Category { get; set; } = string.Empty; // "Demographics", "Interests", "Behavior"
    
    [Required]
    [MaxLength(200)]
    public string Value { get; set; } = string.Empty;
    
    public int Count { get; set; }
    
    public double Percentage { get; set; }
    
    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
    
    [MaxLength(1000)]
    public string? AdditionalData { get; set; } // JSON for complex analytics
}
