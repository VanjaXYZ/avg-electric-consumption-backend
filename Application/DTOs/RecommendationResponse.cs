namespace ElectricityPlanner.Application.DTOs;

public class PlanComparisonDto
{
    public int PlanId { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public CostBreakdown Costs { get; set; } = new CostBreakdown();
}

public class RecommendationResponse
{
    public PlanComparisonDto Recommended { get; set; } = null!;
    public List<PlanComparisonDto> AllPlans { get; set; } = new();
}