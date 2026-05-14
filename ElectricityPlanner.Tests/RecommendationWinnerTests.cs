using ElectricityPlanner.Application.Services;
using ElectricityPlanner.Domain.Entities;

namespace ElectricityPlanner.Tests;

public class RecommendationWinnerTests
{
    [Fact]
    public void At350kWhHousehold_Premium_Is_Cheaper_Than_Standard()
    {
        var service = new PricingService();

        var standard = new Plan
        {
            Id = 1,
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

        var premium = new Plan
        {
            Id = 2,
            Name = "Premium",
            Discount = 0.10m,
            PricingTiers =
            [
                new PricingTier { Threshold = 200, PricePerKwh = 0.09m },
                new PricingTier { Threshold = 400, PricePerKwh = 0.07m },
                new PricingTier { Threshold = 600, PricePerKwh = 0.05m },
                new PricingTier { Threshold = null, PricePerKwh = 0.04m }
            ]
        };

        var household = new TaxGroup
        {
            Name = "household",
            Vat = 0.17m,
            EcoTax = 0.005m
        };

        const decimal kwh = 350m;

        var standardTotal = service.CalculateCostBreakdown(kwh, standard, household).GrandTotal;
        var premiumTotal = service.CalculateCostBreakdown(kwh, premium, household).GrandTotal;

        Assert.True(premiumTotal < standardTotal);
        // opciono i tačne vrijednosti koje već znaš iz API-ja:
        Assert.Equal(34.836750m, standardTotal);
        Assert.Equal(32.058000m, premiumTotal);
    }
}