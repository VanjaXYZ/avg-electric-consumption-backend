namespace ElectricityPlanner.Domain.Entities;

public class PricingTier 
{
    public int Id { get; set; }
    public int? Threshold { get; set; }
    public decimal PricePerKwh { get; set; }
    public int PlanId { get; set; }
    public Plan? Plan { get; set; }
}