using ElectricityPlanner.Domain.Entities;
namespace ElectricityPlanner.Application.Services;

public class PricingService
{
   public decimal CalculateConsumption(decimal kwh, Plan plan)
   {
    decimal remaining = kwh;
    decimal total = 0m;

    foreach (var tier in plan.PricingTiers.OrderBy(t => t.Threshold ?? int.MaxValue)){
        if (remaining <= 0) break;

        decimal tierLimit = tier.Threshold ?? decimal.MaxValue;
        decimal applicableKwh = Math.Min(remaining, tierLimit);
        total += applicableKwh * tier.PricePerKwh;
        remaining -= applicableKwh;
    }
    total -= total * plan.Discount;
    return total;
   }
}