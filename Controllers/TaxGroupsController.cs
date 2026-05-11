using ElectricityPlanner.Application.DTOs;
using ElectricityPlanner.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ElectricityPlanner.Api.Controllers;

[ApiController]
[Route("tax-groups")]
public class TaxGroupsController : ControllerBase
{
    private readonly AppDbContext _db;

    public TaxGroupsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TaxGroupDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TaxGroupDTO>>> GetAll(CancellationToken cancellationToken
    )
    {
        var groups = await _db.TaxGroups
        .AsNoTracking()
        .Where(t => !t.IsDeleted)
        .OrderBy(t => t.Id)
        .Select(t => new TaxGroupDTO
        {
            Id = t.Id,
            Name = t.Name,
            Vat = t.Vat,
            EcoTax = t.EcoTax
        })
        .ToListAsync(cancellationToken);

        return Ok(groups);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(TaxGroupDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TaxGroupDTO>> Create([FromBody] TaxGroupCreateRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name)) return BadRequest(new { error = "Name is required" });

        if (request.Vat < 0 || request.EcoTax < 0) return BadRequest(new { error = "Vat and EcoTax must be non-negative" });

        var exists = await _db.TaxGroups.AnyAsync(
            t => !t.IsDeleted && t.Name.ToLower() == request.Name.Trim().ToLower(),
            cancellationToken
        );

        if (exists) return BadRequest(new { error = $"Tax group '{request.Name}' already exists" });

        var entity = new ElectricityPlanner.Domain.Entities.TaxGroup
        {
            Name = request.Name.Trim(),
            Vat = request.Vat,
            EcoTax = request.EcoTax,
            IsDeleted = false
        };
        
        _db.TaxGroups.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        var dto = new TaxGroupDTO
        {
            Id = entity.Id,
            Name = entity.Name,
            Vat = entity.Vat,
            EcoTax = entity.EcoTax
        };

        return Created($"/tax-groups/{dto.Id}", dto);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(TaxGroupDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaxGroupDTO>> Update(
        [FromRoute] int id,
        [FromBody] TaxGroupCreateRequest request,
        CancellationToken cancellationToken
    )
    {
        if (string.IsNullOrWhiteSpace(request.Name)) return BadRequest(new { error = "Name is required" });
        if (request.Vat < 0 || request.EcoTax < 0) return BadRequest(new { error = "Vat and EcoTax must be non-negative" });

        var entity = await _db.TaxGroups.FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted, cancellationToken);

        if (entity is null) return NotFound(new {error = $"Tax group with id {id} not found"});

        var name = request.Name.Trim();

        var exists = await _db.TaxGroups.AnyAsync(
            t => !t.IsDeleted && t.Id != id && t.Name.ToLower() == name.ToLower(),
            cancellationToken
        );

        if (exists) return BadRequest(new {error = $"Tax group with name '{name}' already exists"});

        entity.Name = name;
        entity.Vat = request.Vat;
        entity.EcoTax = request.EcoTax;

        await _db.SaveChangesAsync(cancellationToken);

        var dto = new TaxGroupDTO
        {
            Id = entity.Id,
            Name = entity.Name,
            Vat = entity.Vat,
            EcoTax = entity.EcoTax 
        };

        return Ok(dto);
    }


    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete([FromRoute] int id, CancellationToken cancellationToken)
    {
        var entity = await _db.TaxGroups.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if(entity is null) return NotFound(new {error = $"Tax group with id {id} not found"});
        if (entity.IsDeleted) return NoContent();

        entity.IsDeleted = true;
        await _db.SaveChangesAsync(cancellationToken);

        return NoContent();
    }
}