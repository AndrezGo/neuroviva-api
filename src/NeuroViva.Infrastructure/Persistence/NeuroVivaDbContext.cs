using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using NeuroViva.Application.Common.Abstractions;
using NeuroViva.Domain.Abstractions;
using NeuroViva.Domain.Ai;
using NeuroViva.Domain.Appointments;
using NeuroViva.Domain.Billing;
using NeuroViva.Domain.Catalog;
using NeuroViva.Domain.Community;
using NeuroViva.Domain.Content;
using NeuroViva.Domain.HealthMonitoring;
using NeuroViva.Domain.Marketplace;
using NeuroViva.Domain.Medications;
using NeuroViva.Domain.Onboarding;
using NeuroViva.Domain.Patients;
using NeuroViva.Domain.Tenancy;
using NeuroViva.Domain.Users;

namespace NeuroViva.Infrastructure.Persistence;

public sealed class NeuroVivaDbContext : DbContext, IUnitOfWork
{
    private readonly ITenantContext _tenantContext;

    public NeuroVivaDbContext(DbContextOptions<NeuroVivaDbContext> options, ITenantContext tenantContext)
        : base(options) => _tenantContext = tenantContext;

    // Billing
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<PaymentMethod> PaymentMethods => Set<PaymentMethod>();
    public DbSet<Charge> Charges => Set<Charge>();

    // Users
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<Caregiver> Caregivers => Set<Caregiver>();
    public DbSet<UserPreference> UserPreferences => Set<UserPreference>();

    // Catalog
    public DbSet<Disease> Diseases => Set<Disease>();

    // Onboarding
    public DbSet<OnboardingStep> OnboardingSteps => Set<OnboardingStep>();
    public DbSet<OnboardingSession> OnboardingSessions => Set<OnboardingSession>();

    // Patients
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<PatientDoctor> PatientDoctors => Set<PatientDoctor>();
    public DbSet<PatientCaregiver> PatientCaregivers => Set<PatientCaregiver>();
    public DbSet<ClinicalRecord> ClinicalRecords => Set<ClinicalRecord>();

    // Medications
    public DbSet<Medication> Medications => Set<Medication>();
    public DbSet<MedicationLog> MedicationLogs => Set<MedicationLog>();

    // Health Monitoring
    public DbSet<Symptom> Symptoms => Set<Symptom>();
    public DbSet<MoodLog> MoodLogs => Set<MoodLog>();

    // Appointments
    public DbSet<Appointment> Appointments => Set<Appointment>();

    // AI
    public DbSet<AiAnalysis> AiAnalyses => Set<AiAnalysis>();
    public DbSet<Alert> Alerts => Set<Alert>();
    public DbSet<Notification> Notifications => Set<Notification>();

    // Content
    public DbSet<Resource> Resources => Set<Resource>();
    public DbSet<ApprovalFlow> ApprovalFlows => Set<ApprovalFlow>();
    public DbSet<ResourcePatient> ResourcePatients => Set<ResourcePatient>();
    public DbSet<CaregiverCourse> CaregiverCourses => Set<CaregiverCourse>();
    public DbSet<CourseProgress> CourseProgresses => Set<CourseProgress>();

    // Community
    public DbSet<Group> Groups => Set<Group>();
    public DbSet<GroupMember> GroupMembers => Set<GroupMember>();
    public DbSet<GroupMessage> GroupMessages => Set<GroupMessage>();
    public DbSet<GroupReaction> GroupReactions => Set<GroupReaction>();
    public DbSet<CommunityPost> CommunityPosts => Set<CommunityPost>();
    public DbSet<CommunityComment> CommunityComments => Set<CommunityComment>();
    public DbSet<CommunityReaction> CommunityReactions => Set<CommunityReaction>();

    // Marketplace
    public DbSet<MarketplaceStore> MarketplaceStores => Set<MarketplaceStore>();
    public DbSet<MarketplaceApproval> MarketplaceApprovals => Set<MarketplaceApproval>();
    public DbSet<StoreTag> StoreTags => Set<StoreTag>();
    public DbSet<StoreReport> StoreReports => Set<StoreReport>();

    public async Task<IUnitOfWorkTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        var tx = await Database.BeginTransactionAsync(cancellationToken);
        return new EfUnitOfWorkTransaction(tx);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NeuroVivaDbContext).Assembly);

        // Only tables that actually have tenant_id get the global filter
        if (_tenantContext.TenantId.HasValue)
        {
            var tenantId = _tenantContext.TenantId.Value;
            modelBuilder.Entity<User>().HasQueryFilter(e => e.TenantId == tenantId);
            modelBuilder.Entity<Patient>().HasQueryFilter(e => e.TenantId == tenantId);
            modelBuilder.Entity<Subscription>().HasQueryFilter(e => e.TenantId == tenantId);
            modelBuilder.Entity<PaymentMethod>().HasQueryFilter(e => e.TenantId == tenantId);
        }
    }

    private sealed class EfUnitOfWorkTransaction : IUnitOfWorkTransaction
    {
        private readonly IDbContextTransaction _tx;

        public EfUnitOfWorkTransaction(IDbContextTransaction tx) => _tx = tx;

        public Task CommitAsync(CancellationToken cancellationToken = default)
            => _tx.CommitAsync(cancellationToken);

        public Task RollbackAsync(CancellationToken cancellationToken = default)
            => _tx.RollbackAsync(cancellationToken);

        public ValueTask DisposeAsync() => _tx.DisposeAsync();
    }
}
