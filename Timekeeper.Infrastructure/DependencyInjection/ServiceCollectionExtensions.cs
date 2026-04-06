using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Timekeeper.Application.Abstractions;
using Timekeeper.Application.Payroll;
using Timekeeper.Infrastructure.Persistence;
using Timekeeper.Infrastructure.Services;

namespace Timekeeper.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTimekeeperInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Timekeeper")
            ?? "Host=localhost;Port=5432;Database=timekeeper;Username=postgres;Password=postgres";

        services.AddDbContext<TimekeeperDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IWorkspaceQueryService, WorkspaceQueryService>();
        services.AddScoped<TaxRuleEngine>();
        services.AddScoped<DatabaseBootstrapper>();

        return services;
    }
}
