namespace ElectricityPlanner.Application.DTOs;

public class TaxGroupCreateRequest
{
    public string Name { get; set; } = string.Empty;
    public decimal Vat { get; set; }
    public decimal EcoTax { get; set; }
}