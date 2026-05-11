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

    [HttpGet("plan-selections/trends")]
    [ProducesResponseType(typeof(IReadOnlyList<PlanSelectionTrendDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<PlanSelectionTrendDto>>> PlanSelectionTrends(
        [FromQuery] DateOnly from,
        [FromQuery] DateOnly to,
        CancellationToken cancellationToken)
        {
             if (from > to) return BadRequest(new { error = "'from' must be on or before 'to'." });

    const int maxDays = 366;
    if (to.DayNumber - from.DayNumber > maxDays) return BadRequest(new { error = $"Date range must not exceed {maxDays} days." });

    var startUtc = DateTime.SpecifyKind(from.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
    var endExclusiveUtc = DateTime.SpecifyKind(to.AddDays(1).ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
    var rows = await _db.PlanSelectionEvents
        .AsNoTracking()
        .Where(e => e.CreatedAtUtc >= startUtc && e.CreatedAtUtc < endExclusiveUtc)
        .GroupBy(e => new
        {
            Day = e.CreatedAtUtc.Date,
            e.RecommendedPlanId,
            PlanName = e.RecommendedPlan.Name,
        })
        .Select(g => new PlanSelectionTrendDto
        {
            Date = g.Key.Day,
            PlanId = g.Key.RecommendedPlanId,
            PlanName = g.Key.PlanName,
            Count = g.Count(),
        })
        .OrderBy(x => x.Date)
        .ThenBy(x => x.PlanName)
        .ToListAsync(cancellationToken);
    return Ok(rows);
        }
    
}