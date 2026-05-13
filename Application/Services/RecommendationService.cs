using ElectricityPlanner.Application.DTOs;
using ElectricityPlanner.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ElectricityPlanner.Application.Services;

public class RecommendationService
{
    private readonly AppDbContext _db;
    private readonly PricingService _pricing;

    public RecommendationService(AppDbContext db, PricingService pricing)
    {
        _db = db;
        _pricing = pricing;
    }

    public async Task<(RecommendationResponse? Response, int? TaxGroupId, string? Error)> BuildAsync(
        decimal kwh,
        string taxGroupName,
        CancellationToken cancellationToken = default)
    {
        if (kwh <= 0) return (null, null, "Kwh must be greater than 0");
        if (string.IsNullOrWhiteSpace(taxGroupName)) return (null, null, "Tax group is required");

        var tax = await _db.TaxGroups
            .AsNoTracking()
            .FirstOrDefaultAsync(
                t => !t.IsDeleted && t.Name.ToLower() == taxGroupName.Trim().ToLower(),
                cancellationToken);

        if (tax is null) return (null, null, $"Unknown tax group: {taxGroupName}");

        var plans = await _db.Plans
            .AsNoTracking()
            .Where(p => !p.IsDeleted)
            .Include(p => p.PricingTiers)
            .ToListAsync(cancellationToken);

        if (plans.Count == 0) return (null, null, "No plans found");

        var all = plans.Select(p => new PlanComparisonDto
        {
            PlanId = p.Id,
            PlanName = p.Name,
            Costs = _pricing.CalculateCostBreakdown(kwh, p, tax)
        }).OrderBy(p => p.Costs.GrandTotal).ToList();

        var response = new RecommendationResponse
        {
            Recommended = all.First(),
            AllPlans = all
        };

        return (response, tax.Id, null);
    }
}
