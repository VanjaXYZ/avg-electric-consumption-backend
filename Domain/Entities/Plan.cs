namespace ElectricityPlanner.Domain.Entities;

public class Plan
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Discount { get; set; }
    public List<PricingTier> PricingTiers { get; set; } = new List<PricingTier>();
}