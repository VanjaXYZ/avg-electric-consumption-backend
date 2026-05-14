using ElectricityPlanner.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ElectricityPlanner.Tests.Support;

internal static class InMemoryDb
{
    public static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }
}
