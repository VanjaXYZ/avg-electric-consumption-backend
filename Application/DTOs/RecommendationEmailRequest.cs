namespace ElectricityPlanner.Application.DTOs;

public class RecommendationEmailRequest
{
    public decimal Kwh { get; set; }
    public string TaxGroup { get; set; } = string.Empty;
    public string ToEmail { get; set; } = string.Empty;
}
