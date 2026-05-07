namespace ElectricityPlanner.Application.DTOs;

public class CalculateRequest
{
    public decimal Kwh { get; set; }
    public string PlanName { get; set; }
}