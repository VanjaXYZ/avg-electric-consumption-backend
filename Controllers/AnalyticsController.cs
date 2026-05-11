using ElectricityPlanner.Application.DTOs;
using ElectricityPlanner.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ElectricityPlanner.Api.Controllers;

[ApiController]
[Route("analytics")]
[Authorize(Roles = "Admin")]
public class AnalyticsController : ControllerBase
{
    private readonly AppDbContext _db;

    public AnalyticsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet("plan-selections/summary")]
    [ProducesResponseType(typeof(IReadOnlyList<PlanSelectionSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PlanSelectionSummaryDto>>> PlanSelectionSummary(
        CancellationToken cancellationToken)
    {
        var rows = await _db.PlanSelectionEvents
            .AsNoTracking()
            .GroupBy(e => new { e.RecommendedPlanId, e.RecommendedPlan.Name })
            .Select(g => new PlanSelectionSummaryDto
            {
                PlanId = g.Key.RecommendedPlanId,
                PlanName = g.Key.Name,
                SelectionCount = g.Count(),
            })
            .OrderByDescending(x => x.SelectionCount)
            .ToListAsync(cancellationToken);

        return Ok(rows);
    }
}