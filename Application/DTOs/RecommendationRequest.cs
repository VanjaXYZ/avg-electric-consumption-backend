namespace ElectricityPlanner.Application.DTOs;

public class RecommendationRequest
{
    public decimal Kwh { get; set; }
    public string TaxGroup { get; set; } = string.Empty;
}