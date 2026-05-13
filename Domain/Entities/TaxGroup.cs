namespace ElectricityPlanner.Domain.Entities;

public class TaxGroup
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Vat { get; set; }
    public decimal EcoTax { get; set; }
    public bool IsDeleted { get; set; }
}