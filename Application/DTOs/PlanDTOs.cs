namespace ElectricityPlanner.Application.DTOs;

public class PlanDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Discount { get; set; }
    public List<PricingTierDTO> PricingTiers { get; set; } = new List<PricingTierDTO>();
}

public class PricingTierDTO
{
    public int Id { get; set; }
    public int? Threshold { get; set; }
    public decimal PricePerKwh { get; set; }
}