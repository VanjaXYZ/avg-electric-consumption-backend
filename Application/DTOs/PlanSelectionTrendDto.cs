namespace ElectricityPlanner.Application.DTOs;

public class PlanSelectionTrendDto
{
    public DateTime Date { get; set; }
    public int PlanId { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public int Count { get; set; }
}