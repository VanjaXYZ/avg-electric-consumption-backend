namespace ElectricityPlanner.Application.DTOs;

public class CostBreakdown 
{
    public decimal EnergySubtotal { get; set; }
    public decimal EnergyAfterDiscount { get; set; }
    public decimal EcoTaxTotal { get; set; }
    public decimal VatAmount { get; set; }
    public decimal GrandTotal { get; set; }
}