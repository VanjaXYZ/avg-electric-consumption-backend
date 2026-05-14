using ElectricityPlanner.Application.Services;
using ElectricityPlanner.Domain.Entities;

namespace ElectricityPlanner.Tests;

public class TierBoundaryTests
{
    private static Plan StandardPlan => new()
    {
        Name = "Standard",
        Discount = 0.05m,
        PricingTiers =
        [
            new PricingTier { Threshold = 100, PricePerKwh = 0.10m },
            new PricingTier { Threshold = 300, PricePerKwh = 0.08m },
            new PricingTier { Threshold = 500, PricePerKwh = 0.07m },
            new PricingTier { Threshold = null, PricePerKwh = 0.06m }
        ]
    };

    private static TaxGroup Household => new()
    {
        Name = "household",
        Vat = 0.17m,
        EcoTax = 0.005m
    };

    [Theory]
    [InlineData(100, 10.00, 11.700000)]
    [InlineData(300, 26.00, 30.654000)]
    [InlineData(500, 40.00, 47.385000)]
    public void StandardPlan_BoundaryKwh_Household(
        decimal kwh,
        decimal expectedEnergySubtotal,
        decimal expectedGrandTotal)
    {
        var service = new PricingService();

        var result = service.CalculateCostBreakdown(kwh, StandardPlan, Household);

        Assert.Equal(expectedEnergySubtotal, result.EnergySubtotal);
        Assert.Equal(expectedGrandTotal, result.GrandTotal);
    }
}
