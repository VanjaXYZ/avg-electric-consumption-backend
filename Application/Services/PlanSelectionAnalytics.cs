using ElectricityPlanner.Application.DTOs;
using ElectricityPlanner.Domain.Entities;
using ElectricityPlanner.Infrastructure.Data;
namespace ElectricityPlanner.Application.Services;


public class PlanSelectionAnalytics : IPlanSelectionAnalytics
{
    private readonly AppDbContext _db;

    public PlanSelectionAnalytics(AppDbContext db)
    {
        _db = db;
    }

    public async Task RecordRecommendationAsync(
        RecommendationRequest request,
        int taxGroupId,
        PlanComparisonDto recommended,
        CancellationToken cancellationToken = default)
        {
            var row = new PlanSelectionEvent
            {
                CreatedAtUtc = DateTime.UtcNow,
                Kwh = request.Kwh,
                TaxGroupId = taxGroupId,
                RecommendedPlanId = recommended.PlanId,
                RecommendedGrandTotal = recommended.Costs.GrandTotal,
            };
            _db.PlanSelectionEvents.Add(row);
            await _db.SaveChangesAsync(cancellationToken);
        }
    
}