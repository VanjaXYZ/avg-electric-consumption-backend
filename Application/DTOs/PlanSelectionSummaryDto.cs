namespace ElectricityPlanner.Application.DTOs;

public class PlanSelectionSummaryDto
{
    public int PlanId { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public int SelectionCount { get; set; }
}