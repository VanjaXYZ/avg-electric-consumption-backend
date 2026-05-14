using ElectricityPlanner.Application.Services;
using ElectricityPlanner.Domain.Entities;
using Xunit;
using System.Collections.Generic;
using System.Linq;

namespace ElectricityPlanner.Tests;

public class PricingServiceTests
{
    [Fact]
    public void CalculateCostBreakdown_StandardPlan_350kWh_Household_MatchesExpected()
    {
        // Arrange
        var service = new PricingService();

        var plan = new Plan
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

        var tax = new TaxGroup
        {
            Name = "household",
            Vat = 0.17m,
            EcoTax = 0.005m
        };

        const decimal kwh = 350m;

        // Act
        var result = service.CalculateCostBreakdown(kwh, plan, tax);

        // Assert (ručno provjereno: isto kao Postman odgovor za Standard)
        Assert.Equal(29.50m, result.EnergySubtotal);
        Assert.Equal(28.0250m, result.EnergyAfterDiscount);
        Assert.Equal(1.750m, result.EcoTaxTotal);
        Assert.Equal(5.061750m, result.VatAmount);
        Assert.Equal(34.836750m, result.GrandTotal);
    }
}



public class PricingServiceTheoryTests
{
    public static Plan StandardPlan => new()
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

    public static TaxGroup HouseholdTax => new()
    {
        Name = "household",
        Vat = 0.17m,
        EcoTax = 0.005m
    };

    [Theory]
    [InlineData(50, 5.00, 5.85)]           // samo prvi pojas
    [InlineData(350, 29.50, 34.836750)]    // kao Postman
    [InlineData(520, 41.20, 48.835800)]    // svi pojasevi uključujući zadnji
    public void CalculateCostBreakdown_MultipleKwh_Household(
        decimal kwh,
        decimal expectedEnergySubtotal,
        decimal expectedGrandTotal)
    {
        var service = new PricingService();

        var result = service.CalculateCostBreakdown(kwh, StandardPlan, HouseholdTax);

        Assert.Equal(expectedEnergySubtotal, result.EnergySubtotal);
        Assert.Equal(expectedGrandTotal, result.GrandTotal);
    }
}