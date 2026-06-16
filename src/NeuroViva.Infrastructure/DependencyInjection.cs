using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NeuroViva.Application.Common.Abstractions;
using NeuroViva.Application.Features.Users.Queries;
using NeuroViva.Domain.Abstractions;
using NeuroViva.Domain.Users.Repositories;
using NeuroViva.Infrastructure.ExternalServices.Clock;
using NeuroViva.Infrastructure.Identity;
using NeuroViva.Infrastructure.Persistence;
using NeuroViva.Infrastructure.ReadRepositories;
using NeuroViva.Infrastructure.Repositories;

namespace NeuroViva.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpContextAccessor();

        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<ITenantContext, TenantContext>();
        services.AddSingleton<IClock, SystemClock>();

        var connectionString = configuration["Database:ConnectionString"]
            ?? throw new InvalidOperationException("Database:ConnectionString is not configured.");

        services.AddDbContext<NeuroVivaDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsql =>
            {
                npgsql.CommandTimeout(
                    configuration.GetValue<int>("Database:CommandTimeoutSeconds", 30));
            });

            if (configuration.GetValue<bool>("Database:EnableSensitiveDataLogging"))
                options.EnableSensitiveDataLogging();
        });

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<NeuroVivaDbContext>());

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();

        // Read repositories
        services.AddScoped<IUserReadRepository, UserReadRepository>();

        return services;
    }
}
