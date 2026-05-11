namespace ElectricityPlanner.Domain.Entities;

public class PlanSelectionEvent
{
    public int Id { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public decimal Kwh { get; set; }

    public int TaxGroupId { get; set; }
    public TaxGroup TaxGroup { get; set; } = null!;

    public int RecommendedPlanId { get; set; }
    public Plan RecommendedPlan { get; set; } = null!;

    public decimal RecommendedGrandTotal { get; set; }
}