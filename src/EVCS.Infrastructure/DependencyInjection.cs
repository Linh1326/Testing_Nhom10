using EVCS.Application.Abstractions.Persistence;
using EVCS.Infrastructure.Persistence;
using EVCS.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EVCS.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Thiếu cấu hình ConnectionStrings:DefaultConnection.");
        }

        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sql =>
            {
                sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
            });
        });

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IStationRepository, StationRepository>();
        services.AddScoped<IPoleRepository, PoleRepository>();
        services.AddScoped<IChargeTypeRepository, ChargeTypeRepository>();
        services.AddScoped<IConnectorRepository, ConnectorRepository>();
        services.AddScoped<IChargingSessionRepository, ChargingSessionRepository>();
        services.AddScoped<IAlertRepository, AlertRepository>();

        return services;
    }
}
