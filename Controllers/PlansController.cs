using ElectricityPlanner.Application.DTOs;
using ElectricityPlanner.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ElectricityPlanner.Api.Controllers;


[ApiController]
[Route("plans")]
public class PlansController : ControllerBase
{
    private readonly AppDbContext _db;
    public PlansController(AppDbContext db)
    {
        _db = db;
    }
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<PlanDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PlanDTO>>> GetAll(CancellationToken cancellationToken)
    {
        var plans = await _db.Plans
            .AsNoTracking()
            .Include(p => p.PricingTiers)
            .OrderBy(p => p.Id)
            .ToListAsync(cancellationToken);
        var response = plans.Select(p => new PlanDTO
        {
            Id = p.Id,
            Name = p.Name,
            Discount = p.Discount,
            PricingTiers = p.PricingTiers
                .OrderBy(t => t.Threshold ?? int.MaxValue)
                .Select(t => new PricingTierDTO
                {
                    Id = t.Id,
                    Threshold = t.Threshold,
                    PricePerKwh = t.PricePerKwh
                })
                .ToList()
        }).ToList();
        return Ok(response);
    }
}