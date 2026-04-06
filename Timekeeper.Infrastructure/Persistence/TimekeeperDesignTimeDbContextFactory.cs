using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Timekeeper.Infrastructure.Persistence;

public sealed class TimekeeperDesignTimeDbContextFactory : IDesignTimeDbContextFactory<TimekeeperDbContext>
{
    public TimekeeperDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("TIMEKEEPER_CONNECTION")
            ?? "Host=localhost;Port=5432;Database=timekeeper;Username=postgres;Password=postgres";

        var builder = new DbContextOptionsBuilder<TimekeeperDbContext>();
        builder.UseNpgsql(connectionString);
        return new TimekeeperDbContext(builder.Options);
    }
}
