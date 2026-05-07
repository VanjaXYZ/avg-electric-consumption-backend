namespace ElectricityPlanner.Infrastructure.Data;

using ElectricityPlanner.Domain.Entities;
using Microsoft.EntityFrameworkCore;
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Plan> Plans { get; set; }
    public DbSet<PricingTier> PricingTiers { get; set; }
    public DbSet<TaxGroup> TaxGroups { get; set; }
}