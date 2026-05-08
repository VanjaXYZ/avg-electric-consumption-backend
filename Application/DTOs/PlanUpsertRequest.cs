namespace ElectricityPlanner.Application.DTOs;

public class PlanUpsertRequest
{
    public string Name { get; set; } = string.Empty;
    public decimal Discount { get; set; }
    public List<PricingTierUpsertRequest> PricingTiers { get; set; } = new();
}

public class PricingTierUpsertRequest
{
    public int? Threshold { get; set; }
    public decimal PricePerKwh { get; set; }
}