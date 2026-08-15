using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NeuroViva.Application.Ai;
using NeuroViva.Application.Ai.Services;
using NeuroViva.Application.Caregivers;
using NeuroViva.Application.Common.Abstractions;
using NeuroViva.Application.Common.Options;
using NeuroViva.Application.Common.Services;
using NeuroViva.Application.Doctors;
using NeuroViva.Application.MedicalRecords;
using NeuroViva.Application.Features.Users.Queries;
using NeuroViva.Domain.Abstractions;
using NeuroViva.Domain.Ai.Repositories;
using NeuroViva.Domain.Billing.Repositories;
using NeuroViva.Domain.Community.Repositories;
using NeuroViva.Domain.Content.Repositories;
using NeuroViva.Domain.Catalog.Repositories;
using NeuroViva.Domain.Appointments.Repositories;
using NeuroViva.Domain.HealthMonitoring.Repositories;
using NeuroViva.Domain.Medications.Repositories;
using NeuroViva.Domain.Patients.Repositories;
using NeuroViva.Domain.Tenancy.Repositories;
using NeuroViva.Domain.Users.Repositories;
using NeuroViva.Infrastructure.DomainEvents;
using NeuroViva.Infrastructure.ExternalServices;
using NeuroViva.Infrastructure.ExternalServices.Clock;
using NeuroViva.Infrastructure.Identity;
using NeuroViva.Infrastructure.Persistence;
using NeuroViva.Infrastructure.ReadRepositories;
using NeuroViva.Infrastructure.Repositories;
using NeuroViva.Infrastructure.Storage;

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
        services.AddScoped<IMedicalRecordReadRepository, MedicalRecordReadRepository>();

        // Application services
        services.AddScoped<IPatientAccessGuard, PatientAccessGuard>();

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

        // Content repositories
        services.AddScoped<IResourceRepository, ResourceRepository>();
        services.AddScoped<IChannelRepository, ChannelRepository>();
        services.AddScoped<INewsArticleRepository, NewsArticleRepository>();
        services.AddScoped<IScientificArticleRecordRepository, ScientificArticleRecordRepository>();

        // Community repositories
        services.AddScoped<IGroupRepository, GroupRepository>();
        services.AddScoped<IGroupMemberRepository, GroupMemberRepository>();
        services.AddScoped<ICommunityPostRepository, CommunityPostRepository>();
        services.AddScoped<ICommunityCommentRepository, CommunityCommentRepository>();
        services.AddScoped<ICommunityReactionRepository, CommunityReactionRepository>();

        // Doctor repositories
        services.AddScoped<IDoctorRepository, DoctorRepository>();
        services.AddScoped<IPatientDoctorRepository, PatientDoctorRepository>();
        services.AddScoped<IAlertRepository, AlertRepository>();
        services.AddScoped<IDoctorReadRepository, DoctorReadRepository>();

        // Storage options
        services.Configure<SupabaseStorageOptions>(
            configuration.GetSection(SupabaseStorageOptions.SectionName));

        // Bind StorageOptions and register the plain object so Application handlers can inject it directly
        // without a Microsoft.Extensions.Options reference in the Application project.
        var storageOptions = new StorageOptions();
        configuration.GetSection(StorageOptions.SectionName).Bind(storageOptions);
        services.AddSingleton(storageOptions);

        // Supabase Storage HTTP client (typed client registered for SupabaseStorageService)
        var supabaseUrl = configuration["Supabase:Url"]
            ?? throw new InvalidOperationException("Supabase:Url is not configured.");

        services.AddHttpClient<IStorageService, SupabaseStorageService>(client =>
        {
            client.BaseAddress = new Uri(supabaseUrl);
        });

        // Google News RSS typed client (external service, no auth)
        services.AddHttpClient<IGoogleNewsRssService, GoogleNewsRssService>(client =>
        {
            client.BaseAddress = new Uri("https://news.google.com/");
            client.Timeout = TimeSpan.FromSeconds(10);
        });

        // Europe PMC typed client (external service, no auth)
        services.AddHttpClient<IEuropePmcService, EuropePmcService>(client =>
        {
            client.BaseAddress = new Uri("https://www.ebi.ac.uk/europepmc/webservices/rest/");
            client.Timeout = TimeSpan.FromSeconds(15);
        });

        // Groq AI Chat typed client
        services.Configure<GroqOptions>(configuration.GetSection(GroqOptions.SectionName));

        services.AddHttpClient<IGroqChatService, GroqChatService>((sp, client) =>
        {
            var opts = sp.GetRequiredService<IOptions<GroqOptions>>().Value;
            client.BaseAddress = new Uri(opts.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(opts.TimeoutSeconds);
        });

        // AI chat repositories
        services.AddScoped<IAiChatConversationRepository, AiChatConversationRepository>();
        services.AddScoped<IAiChatMessageRepository, AiChatMessageRepository>();

        // Patient context services
        services.AddScoped<IPatientContextReadRepository, PatientContextReadRepository>();
        services.AddScoped<IPatientContextBuilder, PatientContextBuilder>();

        return services;
    }
}
