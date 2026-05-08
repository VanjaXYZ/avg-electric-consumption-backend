namespace ElectricityPlanner.Application.DTOs;

public class TaxGroupDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Vat {get; set;}
    public decimal EcoTax {get; set;}
}