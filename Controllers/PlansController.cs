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

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(PlanDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PlanDTO>> GetById([FromRoute] int id, CancellationToken cancellationToken)
    {
        var plan = await _db.Plans
        .AsNoTracking()
        .Include(p => p.PricingTiers)
        .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (plan is null) return NotFound(new { error = $"Plan with id {id} not found" });

        var dto = new PlanDTO
        {
            Id = plan.Id,
            Name = plan.Name,
            Discount = plan.Discount,
            PricingTiers = plan.PricingTiers
                .OrderBy(t => t.Threshold ?? int.MaxValue)
                .Select(t => new PricingTierDTO
                {
                    Id = t.Id,
                    Threshold = t.Threshold,
                    PricePerKwh = t.PricePerKwh
                })
                .ToList()
        };
        return Ok(dto);
    }


[HttpPost]
[ProducesResponseType(typeof(PlanDTO), StatusCodes.Status201Created)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
public async Task<ActionResult<PlanDTO>> Create([FromBody] PlanUpsertRequest request, CancellationToken cancellationToken)
{
    if (string.IsNullOrWhiteSpace(request.Name)) return BadRequest(new { error = "Name is required" });
    if (request.Discount < 0 || request.Discount > 1) return BadRequest(new { error = "Discount must be between 0 and 1" });
    if (request.PricingTiers is null || request.PricingTiers.Count == 0) return BadRequest(new { error = "At least one pricing tier is required" });
    if (request.PricingTiers.Any(t => t.PricePerKwh <= 0)) return BadRequest(new { error = "Price per kwh must be positive" });

    var entity = new ElectricityPlanner.Domain.Entities.Plan
    {
        Name = request.Name.Trim(),
        Discount = request.Discount,
        PricingTiers = request.PricingTiers.Select(t => new ElectricityPlanner.Domain.Entities.PricingTier
        {
            Threshold = t.Threshold,
            PricePerKwh = t.PricePerKwh
        }).ToList()
    };

    _db.Plans.Add(entity);
    await _db.SaveChangesAsync(cancellationToken);

  var dto = new PlanDTO
    {
        Id = entity.Id,
        Name = entity.Name,
        Discount = entity.Discount,
        PricingTiers = entity.PricingTiers
            .OrderBy(t => t.Threshold ?? int.MaxValue)
            .Select(t => new PricingTierDTO
            {
                Id = t.Id,
                Threshold = t.Threshold,
                PricePerKwh = t.PricePerKwh
            })
            .ToList()
    };
    return Created($"/plans/{dto.Id}", dto);
}

[HttpPut("{id:int}")]
[ProducesResponseType(typeof(PlanDTO), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<ActionResult<PlanDTO>> Update([FromRoute] int id, [FromBody] PlanUpsertRequest request, CancellationToken cancellationToken)
{
    if (string.IsNullOrWhiteSpace(request.Name)) return BadRequest(new { error = "Name is required" });
    if (request.Discount < 0 || request.Discount > 1) return BadRequest(new { error = "Discount must be between 0 and 1" });
    if (request.PricingTiers is null || request.PricingTiers.Count == 0) return BadRequest(new { error = "At least one pricing tier is required" });
    if (request.PricingTiers.Any(t => t.PricePerKwh <= 0)) return BadRequest(new { error = "Price per kwh must be positive" });

    var plan = await _db.Plans.Include(p => p.PricingTiers).FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    if (plan is null) return NotFound(new { error = $"Plan with id {id} not found" });

    plan.Name = request.Name.Trim();
    plan.Discount = request.Discount;

    plan.PricingTiers.Clear();

    plan.PricingTiers = request.PricingTiers.Select(t => new ElectricityPlanner.Domain.Entities.PricingTier
    {
        Threshold = t.Threshold,
        PricePerKwh = t.PricePerKwh
    }).ToList();

    await _db.SaveChangesAsync(cancellationToken);

    var dto = new PlanDTO
    {
        Id = plan.Id,
        Name = plan.Name,
        Discount = plan.Discount,
        PricingTiers = plan.PricingTiers
            .OrderBy(t => t.Threshold ?? int.MaxValue)
            .Select(t => new PricingTierDTO
            {
                Id = t.Id,
                Threshold = t.Threshold,
                PricePerKwh = t.PricePerKwh
            })
            .ToList()
    };
    return Ok(dto);
}

[HttpDelete("{id:int}")]
[ProducesResponseType(StatusCodes.Status204NoContent)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<ActionResult> Delete([FromRoute] int id, CancellationToken cancellationToken)
{
    var plan = await _db.Plans.FindAsync([id], cancellationToken);
    if (plan is null) return NotFound(new { error = $"Plan with id {id} not found" });
    _db.Plans.Remove(plan);
    await _db.SaveChangesAsync(cancellationToken);
    return NoContent();
}
}

