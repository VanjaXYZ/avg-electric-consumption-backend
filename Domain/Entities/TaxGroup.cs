namespace ElectricityPlanner.Domain.Entities;

public class TaxGroup
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Vat { get; set; }
    public decimal EcoTax { get; set; }
    /// <summary>When true, group is hidden from API lists and recommendation; row kept for analytics FK.</summary>
    public bool IsDeleted { get; set; }
}