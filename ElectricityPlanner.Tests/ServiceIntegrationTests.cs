using ElectricityPlanner.Application.DTOs;
using ElectricityPlanner.Application.Services;
using ElectricityPlanner.Infrastructure.Data;
using ElectricityPlanner.Tests.Support;
using Microsoft.EntityFrameworkCore;

namespace ElectricityPlanner.Tests;

public sealed class ServiceIntegrationTests
{
    [Fact]
    public async Task Recommendation_build_returns_premium_for_household_350kwh()
    {
        await using var db = InMemoryDb.CreateContext();
        await SeedData.SeedAsync(db);
        var sut = new RecommendationService(db, new PricingService());

        var (response, taxGroupId, error) = await sut.BuildAsync(350m, "household");

        Assert.Null(error);
        Assert.NotNull(response);
        Assert.NotNull(taxGroupId);
        Assert.Equal("Premium", response!.Recommended.PlanName);
        Assert.True(response.Recommended.Costs.GrandTotal > 0);
        Assert.Equal(2, response.AllPlans.Count);
    }

    [Fact]
    public async Task Recommendation_build_unknown_tax_group_returns_error()
    {
        await using var db = InMemoryDb.CreateContext();
        await SeedData.SeedAsync(db);
        var sut = new RecommendationService(db, new PricingService());

        var (response, taxGroupId, error) = await sut.BuildAsync(100m, "nonexistent");

        Assert.Null(response);
        Assert.Null(taxGroupId);
        Assert.Contains("Unknown tax group", error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Recommendation_build_non_positive_kwh_returns_error()
    {
        await using var db = InMemoryDb.CreateContext();
        await SeedData.SeedAsync(db);
        var sut = new RecommendationService(db, new PricingService());

        var (response, _, error) = await sut.BuildAsync(0m, "household");

        Assert.Null(response);
        Assert.NotNull(error);
    }

    [Fact]
    public async Task Plan_selection_analytics_persists_event_after_recommendation()
    {
        await using var db = InMemoryDb.CreateContext();
        await SeedData.SeedAsync(db);
        var recommendation = new RecommendationService(db, new PricingService());
        var analytics = new PlanSelectionAnalytics(db);

        var (response, taxGroupId, error) = await recommendation.BuildAsync(350m, "household");
        Assert.Null(error);

        await analytics.RecordRecommendationAsync(
            new RecommendationRequest { Kwh = 350m, TaxGroup = "household" },
            taxGroupId!.Value,
            response!.Recommended);

        Assert.Equal(1, await db.PlanSelectionEvents.CountAsync());
        var row = await db.PlanSelectionEvents.AsNoTracking().SingleAsync();
        Assert.Equal(350m, row.Kwh);
        Assert.Equal(taxGroupId, row.TaxGroupId);
        Assert.Equal(response.Recommended.PlanId, row.RecommendedPlanId);
        Assert.Equal(response.Recommended.Costs.GrandTotal, row.RecommendedGrandTotal);
    }
}
