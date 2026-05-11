using ElectricityPlanner.Application.DTOs;
using ElectricityPlanner.Application.Services;
using ElectricityPlanner.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ElectricityPlanner.Api.Controllers;

[ApiController]
[Route("recommendation")]
public class RecommendationController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly PricingService _pricing;
    private readonly IPlanSelectionAnalytics _analytics;

    public RecommendationController(AppDbContext db, PricingService pricing, IPlanSelectionAnalytics analytics)
    {
        _db = db;
        _pricing = pricing;
        _analytics = analytics;
    }

    [HttpPost]
    [ProducesResponseType(typeof(RecommendationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RecommendationResponse>> Recommend([FromBody] RecommendationRequest request, CancellationToken cancellationToken)
    {
        if (request.Kwh <= 0) return BadRequest(new { error = "Kwh must be greater than 0" });
        if (string.IsNullOrWhiteSpace(request.TaxGroup)) return BadRequest(new { error = "Tax group is required" });

        var tax = await _db.TaxGroups
        .AsNoTracking()
        .FirstOrDefaultAsync(
            t => !t.IsDeleted && t.Name.ToLower() == request.TaxGroup.ToLower(),
            cancellationToken);

        if(tax is null) return BadRequest(new { error = $"Unknown tax group: {request.TaxGroup}" });

        var plans = await _db.Plans
        .AsNoTracking()
        .Where(p => !p.IsDeleted)
        .Include(p => p.PricingTiers)
        .ToListAsync(cancellationToken);

        if (plans.Count == 0) return BadRequest(new { error = "No plans found" });

        var all = plans.Select(p => new PlanComparisonDto
        {
            PlanId = p.Id,
            PlanName = p.Name,
            Costs = _pricing.CalculateCostBreakdown(request.Kwh, p, tax)
        }).OrderBy(p => p.Costs.GrandTotal).ToList();

        var response = new RecommendationResponse
        {
            Recommended = all.First(),
            AllPlans = all
        };

        await _analytics.RecordRecommendationAsync(request, tax.Id, response.Recommended, cancellationToken);

        return Ok(response);
    }
}