namespace ElectricityPlanner.Domain.Entities;

public class Plan
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Discount { get; set; }
    /// <summary>When true, plan is hidden from catalog and recommendation; row kept for analytics FK.</summary>
    public bool IsDeleted { get; set; }
    public List<PricingTier> PricingTiers { get; set; } = new List<PricingTier>();
}