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
    public DbSet<AppUsers> AppUsers { get; set; }
    public DbSet<PlanSelectionEvent> PlanSelectionEvents { get; set; }

 protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<AppUsers>(entity =>
    {
        entity.ToTable("AppUsers");
        entity.HasIndex(e => e.Username).IsUnique();
        entity.Property(e => e.Username).HasMaxLength(64);
        entity.Property(e => e.Role).HasMaxLength(32);
    });

     modelBuilder.Entity<PlanSelectionEvent>(entity =>
    {
        entity.ToTable("PlanSelectionEvents");
        entity.HasIndex(e => e.CreatedAtUtc);
        entity.HasIndex(e => e.RecommendedPlanId);
        entity.HasOne(e => e.TaxGroup)
            .WithMany()
            .HasForeignKey(e => e.TaxGroupId)
            .OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(e => e.RecommendedPlan)
            .WithMany()
            .HasForeignKey(e => e.RecommendedPlanId)
            .OnDelete(DeleteBehavior.Restrict);
    });
}
}

