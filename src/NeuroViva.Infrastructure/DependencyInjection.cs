using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NeuroViva.Application.Caregivers;
using NeuroViva.Application.Common.Abstractions;
using NeuroViva.Application.Doctors;
using NeuroViva.Application.Features.Users.Queries;
using NeuroViva.Domain.Abstractions;
using NeuroViva.Domain.Ai.Repositories;
using NeuroViva.Domain.Billing.Repositories;
using NeuroViva.Domain.Catalog.Repositories;
using NeuroViva.Domain.Appointments.Repositories;
using NeuroViva.Domain.HealthMonitoring.Repositories;
using NeuroViva.Domain.Medications.Repositories;
using NeuroViva.Domain.Patients.Repositories;
using NeuroViva.Domain.Tenancy.Repositories;
using NeuroViva.Domain.Users.Repositories;
using NeuroViva.Infrastructure.DomainEvents;
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
        services.AddScoped<IDomainEventDispatcher, MediatorDomainEventDispatcher>();

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
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
        services.AddScoped<ISubscriptionPlanRepository, SubscriptionPlanRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IUserRoleRepository, UserRoleRepository>();

        // Read repositories
        services.AddScoped<IUserReadRepository, UserReadRepository>();
        services.AddScoped<ICaregiverReadRepository, CaregiverReadRepository>();

        // Caregiver write repositories
        services.AddScoped<ICaregiverRepository, CaregiverRepository>();
        services.AddScoped<IPatientCaregiverRepository, PatientCaregiverRepository>();

        // Patient repository
        services.AddScoped<IPatientRepository, PatientRepository>();
        services.AddScoped<IClinicalRecordRepository, ClinicalRecordRepository>();
        services.AddScoped<IPatientDiseaseRepository, PatientDiseaseRepository>();

        // Appointment repository
        services.AddScoped<IAppointmentRepository, AppointmentRepository>();

        // Medication repositories
        services.AddScoped<IMedicationRepository, MedicationRepository>();
        services.AddScoped<IMedicationLogRepository, MedicationLogRepository>();

        // Disease repository
        services.AddScoped<IDiseaseRepository, DiseaseRepository>();

        // Health Monitoring repositories
        services.AddScoped<ISymptomRepository, SymptomRepository>();

        // Notification repository
        services.AddScoped<INotificationRepository, NotificationRepository>();

        // Doctor repositories
        services.AddScoped<IDoctorRepository, DoctorRepository>();
        services.AddScoped<IPatientDoctorRepository, PatientDoctorRepository>();
        services.AddScoped<IAlertRepository, AlertRepository>();
        services.AddScoped<IDoctorReadRepository, DoctorReadRepository>();

        return services;
    }
}
