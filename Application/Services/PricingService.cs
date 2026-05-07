using ElectricityPlanner.Domain.Entities;
using ElectricityPlanner.Application.DTOs;
namespace ElectricityPlanner.Application.Services;

public class PricingService
{
    private static decimal CalculateEnergySubtotal(decimal kwh, Plan plan)
{
    if (kwh <=0 || plan.PricingTiers.Count == 0) return 0m;
    
    decimal total = 0m;
    decimal previousCeiling = 0m;
    

    foreach (var tier in plan.PricingTiers.OrderBy(t => t.Threshold ?? int.MaxValue))
    {
        decimal ceiling = tier.Threshold ?? decimal.MaxValue;
        if(kwh <= previousCeiling) break;
        decimal kwhInBand = Math.Min(kwh, ceiling) - previousCeiling;
        if(kwhInBand > 0) total += kwhInBand * tier.PricePerKwh;
        previousCeiling = ceiling;
    }
    return total;
}

public CostBreakdown CalculateCostBreakdown(decimal kwh, Plan plan, TaxGroup taxGroup)
{
    decimal energySubtotal = CalculateEnergySubtotal(kwh, plan);
    decimal energyAfterDiscount = energySubtotal * (1m - plan.Discount);
    decimal ecoTotal = taxGroup.EcoTax * kwh;

    decimal taxableAmount = energyAfterDiscount + ecoTotal;
    decimal vatAmount = taxableAmount * taxGroup.Vat;
    decimal grandTotal = taxableAmount + vatAmount;

    return new CostBreakdown
    {
        EnergySubtotal = energySubtotal,
        EnergyAfterDiscount = energyAfterDiscount,
        EcoTaxTotal = ecoTotal,
        VatAmount = vatAmount,
        GrandTotal = grandTotal
    };
}
   public decimal CalculateConsumption(decimal kwh, Plan plan)
   {
    
    return CalculateEnergySubtotal(kwh, plan) * (1m - plan.Discount);

   }
}

