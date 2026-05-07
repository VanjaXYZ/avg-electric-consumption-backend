using ElectricityPlanner.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ElectricityPlanner.Infrastructure.Data;

public static class SeedData
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if(await db.Plans.AnyAsync()) return;

        db.TaxGroups.AddRange(
            new TaxGroup { Name = "household", Vat = 0.17m, EcoTax = 0.005m },
            new TaxGroup { Name = "business", Vat = 0.20m, EcoTax = 0.010m }
        );

        var standard = new Plan { Name = "Standard", Discount = 0.05m };
        var premium = new Plan { Name = "Premium", Discount = 0.10m };

        db.Plans.AddRange(standard, premium);

        await db.SaveChangesAsync();

        db.PricingTiers.AddRange(
            new PricingTier {PlanId = standard.Id, Threshold = 100, PricePerKwh = 0.10m},
            new PricingTier {PlanId = standard.Id, Threshold = 300, PricePerKwh = 0.08m},
            new PricingTier {PlanId = standard.Id, Threshold = 500, PricePerKwh = 0.07m},
            new PricingTier {PlanId = standard.Id, Threshold = null, PricePerKwh = 0.06m},
            new PricingTier {PlanId = premium.Id, Threshold = 200, PricePerKwh = 0.09m},
            new PricingTier {PlanId = premium.Id, Threshold = 400, PricePerKwh = 0.07m},
            new PricingTier {PlanId = premium.Id, Threshold = 600, PricePerKwh = 0.05m},
            new PricingTier {PlanId = premium.Id, Threshold = null, PricePerKwh = 0.04m}
        );
        await db.SaveChangesAsync();
    }
}