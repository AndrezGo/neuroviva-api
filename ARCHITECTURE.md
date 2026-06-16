# NeuroViva API — Arquitectura de Referencia

> Documento de arquitectura para la API REST de NeuroViva (plataforma SaaS multitenant
> de salud digital para enfermedades neurodegenerativas), construida sobre **.NET 10**
> siguiendo los principios de **Clean Architecture**, **CQRS** y **DDD ligero**.
>
> Estado: borrador de arquitectura — v1.0  
> Autoría técnica: Backend Software Architect C#  
> Implementación: Backend Senior Developer  
> DBA: DBA Expert (esquema PostgreSQL en Supabase)

---

## Tabla de contenidos

1. [Visión General](#1-visión-general)
2. [Diagrama de Arquitectura](#2-diagrama-de-arquitectura)
3. [Estructura de la Solución](#3-estructura-de-la-solución)
4. [Capa Domain](#4-capa-domain)
5. [Capa Application](#5-capa-application)
6. [Capa Infrastructure](#6-capa-infrastructure)
7. [Capa API (Presentación)](#7-capa-api-presentación)
8. [Autenticación y Autorización](#8-autenticación-y-autorización)
9. [Multitenancy](#9-multitenancy)
10. [CQRS Pattern](#10-cqrs-pattern)
11. [Manejo de Errores](#11-manejo-de-errores)
12. [Configuración](#12-configuración)
13. [Módulos API — Endpoints](#13-módulos-api--endpoints)
14. [Convenciones de Código](#14-convenciones-de-código)
15. [Roadmap de Implementación](#15-roadmap-de-implementación)

---

## 1. Visión General

### 1.1. Propósito

NeuroViva API es la capa de servicios que da soporte a la plataforma de salud digital
NeuroViva: una solución multitenant, mobile-first, dirigida a pacientes con
enfermedades neurodegenerativas (Alzheimer, Parkinson, ELA, Huntington, Demencia),
sus cuidadores y los profesionales médicos que los acompañan.

La API expone funcionalidad para:

- Gestión de pacientes, cuidadores, médicos y comité científico.
- Registro de historia clínica longitudinal, medicación, síntomas y estado de ánimo.
- Análisis e inferencias mediante IA y alertas multicanal.
- Comunidad, grupos, recursos educativos, cursos y marketplace de tiendas aliadas.
- Suscripciones, trials, métodos de pago y cobros.
- Backoffice administrativo y moderación de contenido.

### 1.2. Principios arquitectónicos

| Principio | Aplicación en NeuroViva |
|-----------|------------------------|
| Separación de responsabilidades | 4 capas: Domain, Application, Infrastructure, API. |
| Inversión de dependencias | Domain define contratos; Infrastructure los implementa. |
| Domain-centric | La lógica clínica vive en el Dominio, no en controladores ni EF. |
| CQRS | Comandos y queries explícitamente separados vía MediatR. |
| Inmutabilidad por defecto | Records y readonly structs para Value Objects. |
| Multitenancy estricto | Aislamiento por `TenantId` en cada operación. |
| Seguridad por diseño | JWT + políticas por rol + filtros automáticos por tenant. |
| Observabilidad | Logging estructurado con Serilog + correlación por request. |
| Testabilidad | Application y Domain sin dependencias de framework. |

### 1.3. Audiencia técnica

- **Backend Software Architect C#** — autoridad técnica; aprueba/rechaza implementación.
- **Backend Senior Developer** — implementa la solución bajo los lineamientos aquí descritos.
- **DBA Expert** — diseña, audita y versiona el esquema PostgreSQL en Supabase.
- **Frontend / Mobile** — consumen la API; reciben contratos versionados (`/api/v1/...`).
- **DevOps / SRE** — gestionan despliegue, secretos y observabilidad.

### 1.4. Restricciones y supuestos

- La base de datos PostgreSQL es **propiedad de Supabase** y su esquema es **fuente de verdad**.
  La API se adapta al esquema existente; cualquier cambio se canaliza vía DBA Expert.
- La autenticación es emitida por **Supabase Auth**. La API **solo valida JWT**, no emite tokens.
- El backend es **stateless**. No mantiene sesión en memoria.
- Todo dato escrito o leído está **siempre acotado a un tenant**.
- Los datos clínicos son **sensibles**; aplicar minimización, control de acceso estricto y auditoría.

---

## 2. Diagrama de Arquitectura

### 2.1. Vista en capas (Clean Architecture / Onion)

```
                       ┌──────────────────────────────┐
                       │      Clientes (Web, App)     │
                       └──────────────┬───────────────┘
                                      │ HTTPS / JWT
                                      ▼
┌──────────────────────────────────────────────────────────────────┐
│                  NeuroViva.API (Presentación)                    │
│  Controllers · Middleware · Filters · ProblemDetails · Versioning│
└──────────────────────────────┬───────────────────────────────────┘
                               │ MediatR (IRequest / INotification)
                               ▼
┌──────────────────────────────────────────────────────────────────┐
│                NeuroViva.Application (Casos de uso)              │
│  Commands · Queries · Handlers · DTOs · Validators · Behaviors   │
│  Interfaces de servicios externos (IAiAnalysisService,           │
│  INotificationService, IClock, ICurrentUser, IUnitOfWork...)     │
└──────────────────────────────┬───────────────────────────────────┘
                               │ depende de
                               ▼
┌──────────────────────────────────────────────────────────────────┐
│                   NeuroViva.Domain (Núcleo)                      │
│  Entities · ValueObjects · Enums · DomainEvents · Specifications │
│  Repositorios (contratos: IPatientRepository, etc.)              │
│  Excepciones del dominio · Reglas de negocio puras               │
└──────────────────────────────────────────────────────────────────┘
                               ▲
                               │ implementa
┌──────────────────────────────┴───────────────────────────────────┐
│             NeuroViva.Infrastructure (Adaptadores)               │
│  EF Core DbContext · Configuraciones · Repositorios · UoW        │
│  Dapper queries · Supabase Auth Validator · Storage · Mail       │
│  IA Provider · Push/SMS · Migrations (gestionadas por DBA)       │
└──────────────────────────────┬───────────────────────────────────┘
                               │
                               ▼
                  ┌────────────────────────────┐
                  │   PostgreSQL (Supabase)    │
                  │   Auth · Storage · DB      │
                  └────────────────────────────┘
```

### 2.2. Flujo de una petición típica

```
[HTTP Request /api/v1/patients/{id}/symptoms POST]
     │
     ▼
[Auth Middleware]  ──── valida JWT Supabase, hidrata ClaimsPrincipal
     │
     ▼
[Tenant Middleware] ── lee tenant_id del claim, lo expone en ITenantContext
     │
     ▼
[Controller PatientsController.RegisterSymptom]
     │ Mapea DTO → Command (RegisterSymptomCommand)
     ▼
[MediatR Pipeline]
     ├── LoggingBehavior        (Serilog: requestId, userId, tenantId)
     ├── ValidationBehavior     (FluentValidation)
     ├── TenantGuardBehavior    (rechaza si no hay tenant)
     ├── AuthorizationBehavior  (verifica rol/política)
     ├── TransactionBehavior    (abre IDbContextTransaction si Command)
     └── PerformanceBehavior    (warn si >500ms)
     │
     ▼
[RegisterSymptomCommandHandler]
     ├── _patientRepo.GetByIdAsync(...)         (Domain abstraction)
     ├── patient.RegisterSymptom(...)           (lógica en Dominio)
     ├── _unitOfWork.SaveChangesAsync()
     └── publica SymptomRegisteredDomainEvent
     │
     ▼
[NotificationHandler]  ── encola alerta IA si intensidad >= umbral
     │
     ▼
[Controller] → Result<SymptomDto> → 201 Created
```

---

## 3. Estructura de la Solución

```
neuroviva-api/
├── ARCHITECTURE.md                          ← este documento
├── NeuroViva.sln
├── Directory.Build.props                    ← TargetFramework y nullable centralizado
├── Directory.Packages.props                 ← CPM (Central Package Management)
├── .editorconfig
├── .gitignore
│
├── src/
│   │
│   ├── NeuroViva.Domain/
│   │   ├── NeuroViva.Domain.csproj
│   │   ├── Common/
│   │   │   ├── Entity.cs                    ← base abstract Entity<TId>
│   │   │   ├── AggregateRoot.cs             ← marca de raíz de agregado
│   │   │   ├── ValueObject.cs               ← base con igualdad estructural
│   │   │   ├── IDomainEvent.cs
│   │   │   ├── ITenantOwned.cs              ← contrato de pertenencia a tenant
│   │   │   ├── IAuditable.cs                ← createdAt/updatedAt/createdBy
│   │   │   └── ISoftDeletable.cs
│   │   ├── Tenancy/
│   │   │   ├── Tenant.cs
│   │   │   └── TenantId.cs                  ← Value Object Guid-based
│   │   ├── Users/
│   │   │   ├── User.cs (AggregateRoot)
│   │   │   ├── Role.cs
│   │   │   ├── UserRole.cs
│   │   │   ├── Caregiver.cs
│   │   │   ├── Doctor.cs
│   │   │   ├── Enums/RoleType.cs
│   │   │   ├── Events/UserRegisteredDomainEvent.cs
│   │   │   └── Repositories/IUserRepository.cs
│   │   ├── Billing/
│   │   │   ├── SubscriptionPlan.cs
│   │   │   ├── Subscription.cs (AggregateRoot)
│   │   │   ├── PaymentMethod.cs
│   │   │   ├── Charge.cs
│   │   │   ├── Enums/SubscriptionStatus.cs
│   │   │   ├── Enums/PaymentMethodType.cs
│   │   │   ├── Enums/ChargeStatus.cs
│   │   │   ├── ValueObjects/Money.cs        ← (amount, currency)
│   │   │   └── Repositories/ISubscriptionRepository.cs
│   │   ├── Catalog/
│   │   │   ├── Disease.cs
│   │   │   ├── Enums/DiseaseCategory.cs
│   │   │   └── Repositories/IDiseaseRepository.cs
│   │   ├── Onboarding/
│   │   │   ├── OnboardingStep.cs
│   │   │   ├── OnboardingSession.cs (AggregateRoot)
│   │   │   ├── UserPreference.cs
│   │   │   └── Repositories/IOnboardingRepository.cs
│   │   ├── Patients/
│   │   │   ├── Patient.cs (AggregateRoot)
│   │   │   ├── PatientDoctor.cs
│   │   │   ├── PatientCaregiver.cs
│   │   │   ├── ClinicalRecord.cs
│   │   │   ├── Enums/PatientStatus.cs
│   │   │   ├── Enums/ClinicalEventType.cs
│   │   │   ├── ValueObjects/BirthDate.cs
│   │   │   ├── Events/PatientCreatedDomainEvent.cs
│   │   │   └── Repositories/IPatientRepository.cs
│   │   ├── Medications/
│   │   │   ├── Medication.cs (AggregateRoot)
│   │   │   ├── MedicationIntake.cs
│   │   │   ├── ValueObjects/Dose.cs
│   │   │   ├── ValueObjects/Frequency.cs
│   │   │   └── Repositories/IMedicationRepository.cs
│   │   ├── HealthMonitoring/
│   │   │   ├── Symptom.cs (AggregateRoot)
│   │   │   ├── Mood.cs
│   │   │   ├── ValueObjects/Intensity.cs    ← 1..10
│   │   │   ├── ValueObjects/MoodLevel.cs    ← 1..5
│   │   │   ├── Events/HighIntensitySymptomDomainEvent.cs
│   │   │   └── Repositories/ISymptomRepository.cs
│   │   ├── Appointments/
│   │   │   ├── Appointment.cs (AggregateRoot)
│   │   │   ├── Enums/AppointmentType.cs
│   │   │   ├── Enums/AppointmentStatus.cs
│   │   │   └── Repositories/IAppointmentRepository.cs
│   │   ├── Ai/
│   │   │   ├── AiAnalysis.cs (AggregateRoot)
│   │   │   ├── Alert.cs
│   │   │   ├── Notification.cs
│   │   │   ├── Enums/AnalysisType.cs
│   │   │   ├── Enums/AlertPriority.cs
│   │   │   ├── Enums/OverallStatus.cs
│   │   │   ├── Enums/NotificationChannel.cs
│   │   │   └── Repositories/IAiAnalysisRepository.cs
│   │   ├── Resources/
│   │   │   ├── Resource.cs (AggregateRoot)
│   │   │   ├── ApprovalFlow.cs
│   │   │   ├── PatientResource.cs
│   │   │   ├── Enums/ResourceType.cs
│   │   │   ├── Enums/ApprovalStatus.cs
│   │   │   └── Repositories/IResourceRepository.cs
│   │   ├── Courses/
│   │   │   ├── CaregiverCourse.cs
│   │   │   ├── CourseProgress.cs
│   │   │   ├── Enums/CourseType.cs
│   │   │   └── Repositories/ICourseRepository.cs
│   │   ├── Groups/
│   │   │   ├── Group.cs (AggregateRoot)
│   │   │   ├── GroupMember.cs
│   │   │   ├── GroupMessage.cs
│   │   │   ├── GroupReaction.cs
│   │   │   ├── Enums/GroupVisibility.cs
│   │   │   ├── Enums/MessageType.cs
│   │   │   └── Repositories/IGroupRepository.cs
│   │   ├── Community/
│   │   │   ├── CommunityPost.cs (AggregateRoot)
│   │   │   ├── CommunityComment.cs
│   │   │   ├── CommunityReaction.cs
│   │   │   ├── Enums/PostVisibility.cs
│   │   │   ├── Enums/ReactionType.cs
│   │   │   └── Repositories/ICommunityRepository.cs
│   │   ├── Marketplace/
│   │   │   ├── MarketplaceStore.cs (AggregateRoot)
│   │   │   ├── MarketplaceApproval.cs
│   │   │   ├── StoreTag.cs
│   │   │   ├── StoreReport.cs
│   │   │   ├── Enums/StoreApprovalStatus.cs
│   │   │   ├── Enums/ReportReason.cs
│   │   │   └── Repositories/IMarketplaceRepository.cs
│   │   ├── Exceptions/
│   │   │   ├── DomainException.cs
│   │   │   ├── BusinessRuleViolationException.cs
│   │   │   └── EntityNotFoundException.cs
│   │   └── Abstractions/
│   │       ├── IUnitOfWork.cs
│   │       └── IDomainEventDispatcher.cs
│   │
│   ├── NeuroViva.Application/
│   │   ├── NeuroViva.Application.csproj
│   │   ├── Common/
│   │   │   ├── Behaviors/
│   │   │   │   ├── LoggingBehavior.cs
│   │   │   │   ├── ValidationBehavior.cs
│   │   │   │   ├── TenantGuardBehavior.cs
│   │   │   │   ├── AuthorizationBehavior.cs
│   │   │   │   ├── TransactionBehavior.cs
│   │   │   │   └── PerformanceBehavior.cs
│   │   │   ├── Models/
│   │   │   │   ├── Result.cs                ← Result pattern genérico
│   │   │   │   ├── Error.cs
│   │   │   │   ├── PagedResult.cs
│   │   │   │   └── PaginationParams.cs
│   │   │   ├── Mappings/
│   │   │   │   └── MappingConfig.cs         ← Mapster TypeAdapterConfig
│   │   │   ├── Abstractions/
│   │   │   │   ├── ICurrentUserService.cs
│   │   │   │   ├── ITenantContext.cs
│   │   │   │   ├── IClock.cs
│   │   │   │   ├── IAiAnalysisService.cs
│   │   │   │   ├── INotificationDispatcher.cs
│   │   │   │   ├── IPushNotificationSender.cs
│   │   │   │   ├── IEmailSender.cs
│   │   │   │   ├── ISmsSender.cs
│   │   │   │   ├── IStorageService.cs
│   │   │   │   ├── IPaymentGateway.cs
│   │   │   │   └── IDapperQueryRunner.cs
│   │   │   ├── Authorization/
│   │   │   │   ├── IRequireRole.cs
│   │   │   │   └── Policies.cs
│   │   │   └── Exceptions/
│   │   │       ├── ValidationException.cs
│   │   │       ├── UnauthorizedAppException.cs
│   │   │       └── NotFoundAppException.cs
│   │   ├── Auth/
│   │   │   ├── Commands/RegisterUser/
│   │   │   ├── Commands/CompleteOnboarding/
│   │   │   ├── Queries/GetCurrentUser/
│   │   │   └── Dtos/UserDto.cs
│   │   ├── Patients/
│   │   │   ├── Commands/CreatePatient/
│   │   │   ├── Commands/UpdatePatient/
│   │   │   ├── Commands/AssignDoctor/
│   │   │   ├── Commands/AssignCaregiver/
│   │   │   ├── Queries/GetPatientById/
│   │   │   ├── Queries/ListPatients/
│   │   │   └── Dtos/PatientDto.cs
│   │   ├── Medications/
│   │   ├── HealthMonitoring/
│   │   ├── Appointments/
│   │   ├── Ai/
│   │   ├── Resources/
│   │   ├── Courses/
│   │   ├── Groups/
│   │   ├── Community/
│   │   ├── Marketplace/
│   │   ├── Billing/
│   │   ├── Admin/
│   │   └── DependencyInjection.cs
│   │
│   ├── NeuroViva.Infrastructure/
│   │   ├── NeuroViva.Infrastructure.csproj
│   │   ├── Persistence/
│   │   │   ├── NeuroVivaDbContext.cs
│   │   │   ├── Configurations/
│   │   │   │   ├── TenantConfiguration.cs
│   │   │   │   ├── UserConfiguration.cs
│   │   │   │   ├── PatientConfiguration.cs
│   │   │   │   └── ... (uno por entidad)
│   │   │   ├── Interceptors/
│   │   │   │   ├── AuditableEntityInterceptor.cs
│   │   │   │   ├── TenantFilterInterceptor.cs
│   │   │   │   └── DomainEventDispatcherInterceptor.cs
│   │   │   ├── Repositories/
│   │   │   │   ├── PatientRepository.cs
│   │   │   │   ├── UserRepository.cs
│   │   │   │   └── ...
│   │   │   ├── Queries/
│   │   │   │   ├── DapperQueryRunner.cs
│   │   │   │   └── Reads/
│   │   │   ├── UnitOfWork/
│   │   │   │   └── EfUnitOfWork.cs
│   │   │   └── Conventions/
│   │   │       └── SnakeCaseNamingConvention.cs
│   │   ├── Identity/
│   │   │   ├── SupabaseJwtValidator.cs
│   │   │   ├── CurrentUserService.cs
│   │   │   └── TenantContext.cs
│   │   ├── ExternalServices/
│   │   │   ├── Ai/
│   │   │   │   └── OpenAiAnalysisService.cs
│   │   │   ├── Notifications/
│   │   │   │   ├── FirebasePushSender.cs
│   │   │   │   ├── SmtpEmailSender.cs
│   │   │   │   └── TwilioSmsSender.cs
│   │   │   ├── Storage/
│   │   │   │   └── SupabaseStorageService.cs
│   │   │   ├── Payments/
│   │   │   │   └── WompiPaymentGateway.cs
│   │   │   └── Clock/
│   │   │       └── SystemClock.cs
│   │   ├── Logging/
│   │   │   └── SerilogConfiguration.cs
│   │   └── DependencyInjection.cs
│   │
│   └── NeuroViva.API/                        ← proyecto existente (a renombrar)
│       ├── NeuroViva.API.csproj
│       ├── Program.cs
│       ├── appsettings.json
│       ├── appsettings.Development.json
│       ├── Controllers/
│       │   ├── V1/
│       │   │   ├── AuthController.cs
│       │   │   ├── OnboardingController.cs
│       │   │   ├── PatientsController.cs
│       │   │   ├── MedicationsController.cs
│       │   │   ├── HealthMonitoringController.cs
│       │   │   ├── AppointmentsController.cs
│       │   │   ├── AiAnalysisController.cs
│       │   │   ├── AlertsController.cs
│       │   │   ├── NotificationsController.cs
│       │   │   ├── ResourcesController.cs
│       │   │   ├── CoursesController.cs
│       │   │   ├── GroupsController.cs
│       │   │   ├── CommunityController.cs
│       │   │   ├── MarketplaceController.cs
│       │   │   ├── BillingController.cs
│       │   │   └── AdminController.cs
│       │   └── BaseApiController.cs
│       ├── Middleware/
│       │   ├── ExceptionHandlingMiddleware.cs
│       │   ├── TenantResolutionMiddleware.cs
│       │   ├── RequestLoggingMiddleware.cs
│       │   └── CorrelationIdMiddleware.cs
│       ├── Filters/
│       │   └── ApiKeyAuthorizationFilter.cs
│       └── Extensions/
│           ├── ServiceCollectionExtensions.cs
│           ├── SwaggerExtensions.cs
│           ├── AuthExtensions.cs
│           └── VersioningExtensions.cs
│
└── tests/
    ├── NeuroViva.Domain.UnitTests/
    ├── NeuroViva.Application.UnitTests/
    └── NeuroViva.Infrastructure.IntegrationTests/
```

### 3.1. Reglas de dependencias entre proyectos

| Proyecto | Referencia permitida a |
|----------|------------------------|
| `NeuroViva.Domain` | (ninguno) |
| `NeuroViva.Application` | `Domain` |
| `NeuroViva.Infrastructure` | `Application`, `Domain` |
| `NeuroViva.API` | `Application`, `Infrastructure` (solo composition root) |

**Regla dura:** `Domain` y `Application` no pueden referenciar EF Core, ASP.NET,
Supabase, ni paquetes específicos de infraestructura.

---

## 4. Capa Domain

La capa de Dominio contiene el modelo de negocio puro y sus invariantes. No depende
de ningún framework.

### 4.1. Entidades base

```csharp
public abstract class Entity<TId> where TId : notnull
{
    public TId Id { get; protected set; } = default!;
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    protected void RaiseEvent(IDomainEvent @event) => _domainEvents.Add(@event);
    public void ClearDomainEvents() => _domainEvents.Clear();
}

public abstract class AggregateRoot<TId> : Entity<TId> where TId : notnull { }
```

### 4.2. Contratos transversales

```csharp
public interface ITenantOwned { Guid TenantId { get; } }
public interface IAuditable
{
    DateTime CreatedAt { get; }
    DateTime? UpdatedAt { get; }
    Guid? CreatedBy { get; }
    Guid? UpdatedBy { get; }
}
public interface ISoftDeletable { bool IsDeleted { get; } DateTime? DeletedAt { get; } }
```

### 4.3. Value Objects relevantes

- `Money(decimal Amount, string Currency)` — fija COP por defecto.
- `Intensity` (1..10) — usado por `Symptom`.
- `MoodLevel` (1..5) — usado por `Mood`.
- `Dose`, `Frequency` — usados por `Medication`.
- `BirthDate` — valida que no sea futura.
- `Email`, `PhoneNumber` — validación estructural.
- `TenantId` — wrapper fuertemente tipado de `Guid`.

### 4.4. Enums principales

| Enum | Valores |
|------|---------|
| `RoleType` | Patient, Caregiver, Doctor, ScientificCommittee, Admin |
| `SubscriptionStatus` | Trial, Active, Expired, Cancelled, Paused |
| `PaymentMethodType` | CreditCard, DebitCard, Pse |
| `ChargeStatus` | Pending, Successful, Failed, Refunded |
| `PatientStatus` | Active, Inactive, Discharged |
| `ClinicalEventType` | Consultation, Medication, Symptom, Exam, Note, Other |
| `AppointmentType` | Consultation, Exam, Procedure, Teleconsultation |
| `AppointmentStatus` | Scheduled, Confirmed, Completed, Cancelled |
| `AnalysisType` | Daily, Weekly, Event, Request |
| `OverallStatus` | Stable, Attention, High, Critical |
| `AlertPriority` | Info, Medium, High, Critical |
| `NotificationChannel` | Push, Email, InApp, Sms |
| `ResourceType` | Podcast, VideoPodcast, Article, Book, Game, Other |
| `ApprovalStatus` | Pending, AiReview, InCommittee, Approved, Rejected |
| `CourseType` | Video, Reading, Quiz, Practice |
| `GroupVisibility` | Public, Private |
| `MessageType` | Text, Image, File, System |
| `PostVisibility` | Public, Group, Private |
| `ReactionType` | Like, Heart, Support, Applause |
| `StoreApprovalStatus` | Pending, InReview, Approved, Rejected, Suspended |
| `ReportReason` | Spam, InappropriateContent, Fraud, Misinformation, Other |

### 4.5. Interfaces de repositorio

```csharp
public interface IPatientRepository
{
    Task<Patient?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<bool> ExistsAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<Patient>> ListByDoctorAsync(Guid doctorId, CancellationToken ct);
    Task AddAsync(Patient patient, CancellationToken ct);
    void Update(Patient patient);
    void Remove(Patient patient);
}
```

Los repositorios **no exponen `IQueryable`** hacia Application.

### 4.6. Reglas de negocio en el Dominio

- `Patient.RegisterSymptom(intensity, ...)` valida 1..10 y emite `HighIntensitySymptomDomainEvent` si `intensity >= 8`.
- `Subscription.StartTrial(plan, now)` solo si el tenant no tiene trial previo.
- `Subscription.Activate(now)` exige tarjeta registrada.
- `Appointment.Confirm()` solo desde estado `Scheduled`.
- `Resource.SubmitForReview()` solo si `Status == Pending`.

---

## 5. Capa Application

### 5.1. Estructura por feature (Vertical Slices + CQRS)

```
Application/Patients/Commands/CreatePatient/
   ├── CreatePatientCommand.cs        : IRequest<Result<PatientDto>>
   ├── CreatePatientCommandValidator.cs
   ├── CreatePatientCommandHandler.cs
   └── (mapeos Mapster locales si aplica)
```

### 5.2. Command example

```csharp
public sealed record CreatePatientCommand(
    Guid DiseaseId,
    string Name,
    DateOnly BirthDate
) : IRequest<Result<PatientDto>>, IRequireRole
{
    public string[] AllowedRoles => new[] { Roles.Doctor, Roles.Caregiver, Roles.Admin };
}
```

### 5.3. Pipeline Behaviors (orden de ejecución)

1. `LoggingBehavior` — Serilog scope con `RequestName`, `TenantId`, `UserId`.
2. `TenantGuardBehavior` — exige `TenantId` cuando el request implementa `ITenantScoped`.
3. `AuthorizationBehavior` — verifica `IRequireRole.AllowedRoles`.
4. `ValidationBehavior` — corre validadores FluentValidation.
5. `TransactionBehavior` — abre transacción para `ICommand`.
6. `PerformanceBehavior` — warning si `Elapsed > 500ms`.

### 5.4. Result Pattern

```csharp
public readonly record struct Error(string Code, string Message, ErrorType Type);
public enum ErrorType { Validation, NotFound, Conflict, Unauthorized, Forbidden, Failure }

public sealed class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public Error? Error { get; }
    public static Result<T> Success(T value);
    public static Result<T> Failure(Error error);
}
```

Los handlers **nunca lanzan excepciones** por errores de negocio esperados.

### 5.5. Contratos hacia Infrastructure

Application **declara**; Infrastructure **implementa**:

- `ITenantContext`, `ICurrentUserService`, `IClock`
- `IAiAnalysisService`, `INotificationDispatcher`, `IPushNotificationSender`, `IEmailSender`, `ISmsSender`
- `IStorageService`, `IPaymentGateway`, `IDapperQueryRunner`

---

## 6. Capa Infrastructure

### 6.1. EF Core + Supabase PostgreSQL

- Provider: `Npgsql.EntityFrameworkCore.PostgreSQL`
- `NeuroVivaDbContext` consume `Database:ConnectionString`
- Convención **snake_case** para tablas/columnas (el esquema ya existe así)
- Configuraciones por entidad en `Persistence/Configurations/*Configuration.cs`

```csharp
public sealed class NeuroVivaDbContext : DbContext
{
    private readonly ITenantContext _tenant;
    private readonly IDomainEventDispatcher _events;

    public NeuroVivaDbContext(DbContextOptions<NeuroVivaDbContext> opts,
                              ITenantContext tenant,
                              IDomainEventDispatcher events) : base(opts)
    { _tenant = tenant; _events = events; }

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.ApplyConfigurationsFromAssembly(typeof(NeuroVivaDbContext).Assembly);
        mb.ApplyGlobalQueryFilters(_tenant);   // filtro automático por tenant
        mb.ApplySoftDeleteFilters();
    }
}
```

### 6.2. Filtro global por tenant

Cada entidad `ITenantOwned` recibe automáticamente:
```csharp
HasQueryFilter(e => e.TenantId == _tenant.TenantId)
```

### 6.3. Interceptores

- `AuditableEntityInterceptor` — `CreatedAt/UpdatedAt/CreatedBy/UpdatedBy`
- `DomainEventDispatcherInterceptor` — publica eventos de dominio post-`SaveChanges`

### 6.4. Dapper para queries de lectura

`IDapperQueryRunner` se inyecta en handlers de `Query` para proyecciones complejas.
Las queries SQL viven como constantes en `Reads/*.sql.cs`.

### 6.5. Migraciones

**Responsabilidad del DBA Expert.** La API no genera migraciones automáticas sobre
el esquema productivo. Cambios requieren script SQL (forward + rollback) aprobado.

### 6.6. Servicios externos

- **Supabase Auth**: JWT Bearer con JWKS de `{SupabaseUrl}/auth/v1/keys`
- **Storage**: `SupabaseStorageService`
- **IA**: adapter swappable (OpenAI / Groq)
- **Notificaciones**: Firebase FCM + SMTP + Twilio
- **Pagos**: Wompi/PayU (Colombia)

---

## 7. Capa API (Presentación)

### 7.1. Program.cs

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, lc) => lc.ReadFrom.Configuration(ctx.Configuration));

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddApiServices(builder.Configuration);

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseMiddleware<TenantResolutionMiddleware>();
app.UseAuthorization();
app.MapControllers();
app.Run();
```

### 7.2. BaseApiController

```csharp
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    protected ISender Mediator => HttpContext.RequestServices.GetRequiredService<ISender>();

    protected IActionResult OkOrProblem<T>(Result<T> result) =>
        result.IsSuccess ? Ok(result.Value) : Problem(result);

    protected IActionResult CreatedOrProblem<T>(Result<T> result, string routeName, object routeValues) =>
        result.IsSuccess
            ? CreatedAtRoute(routeName, routeValues, result.Value)
            : Problem(result);

    private IActionResult Problem<T>(Result<T> result) => result.Error!.Value.Type switch
    {
        ErrorType.NotFound     => NotFound(result.Error),
        ErrorType.Validation   => UnprocessableEntity(result.Error),
        ErrorType.Conflict     => Conflict(result.Error),
        ErrorType.Unauthorized => Unauthorized(),
        ErrorType.Forbidden    => Forbid(),
        _                      => StatusCode(500, result.Error)
    };
}
```

### 7.3. Versionado y Swagger

- `Asp.Versioning.Mvc` — versión por URL: `/api/v1/...`
- `Swashbuckle.AspNetCore` — documentación separada por versión

### 7.4. Rate limiting

```
auth:    10 req/min por IP  (endpoints registro/login)
default: 100 req/min por usuario autenticado
ai:      20 req/hora por tenant
```

---

## 8. Autenticación y Autorización

### 8.1. Flujo JWT Supabase

1. Cliente se autentica en Supabase Auth.
2. Supabase devuelve `access_token` JWT.
3. Cliente envía `Authorization: Bearer <token>`.
4. API valida issuer, audience, firma (JWKS), expiración.
5. Claims se mapean a `ClaimsPrincipal`.

### 8.2. Claims esperados del JWT de Supabase

```json
{
  "sub": "supabase-auth-user-id",
  "email": "usuario@example.com",
  "app_metadata": {
    "tenant_id": "uuid-del-tenant",
    "roles": ["medico", "comite_cientifico"]
  },
  "exp": 1700000000
}
```

### 8.3. ICurrentUserService

```csharp
public interface ICurrentUserService
{
    Guid? UserId { get; }        // usuario.id en la BD
    Guid? AuthUserId { get; }    // claim 'sub' de Supabase
    Guid? TenantId { get; }
    IReadOnlySet<string> Roles { get; }
    bool IsAuthenticated { get; }
    bool IsInRole(string role);
}
```

### 8.4. Políticas por rol

```csharp
options.AddPolicy(Policies.DoctorOnly, p => p.RequireRole(Roles.Doctor));
options.AddPolicy(Policies.CaregiverOrDoctor, p =>
    p.RequireAssertion(c => c.User.IsInRole(Roles.Caregiver)
                         || c.User.IsInRole(Roles.Doctor)));
options.AddPolicy(Policies.AdminOnly, p => p.RequireRole(Roles.Admin));
options.AddPolicy(Policies.ScientificCommittee, p => p.RequireRole(Roles.ScientificCommittee));
```

---

## 9. Multitenancy

### 9.1. Modelo

Cada usuario y cada dato transaccional pertenecen a un `Tenant`. El `tenant_id`
aísla completamente los datos entre organizaciones.

### 9.2. Resolución del tenant (TenantResolutionMiddleware)

Orden de precedencia:
1. Claim `app_metadata.tenant_id` del JWT (caso normal).
2. Header `X-Tenant-Id` (solo para `admin` global — impersonation).
3. Webhooks (Wompi/Supabase): tenant extraído del payload firmado.

### 9.3. Enforcement

- Filtro global EF Core: ninguna fila de otro tenant se devuelve.
- `TenantGuardBehavior`: rechaza requests sin tenant para módulos `ITenantScoped`.
- Operaciones `admin` global usan `IgnoreQueryFilters()` explícitamente.

---

## 10. CQRS Pattern

### 10.1. Convenciones

| Tipo | Sufijo | Retorno |
|------|--------|---------|
| Command | `...Command` | `Result<TDto>` o `Result` |
| Query | `...Query` | `Result<TDto>` o `Result<PagedResult<TDto>>` |
| Handler (Command) | `...CommandHandler` | acorde |
| Handler (Query) | `...QueryHandler` | acorde |
| Validator | `...CommandValidator` | `AbstractValidator<TCommand>` |
| Domain Event | `...DomainEvent` | implementa `IDomainEvent` |

### 10.2. Reglas

- Un handler por request. Sin lógica compartida entre handlers.
- Commands cambian estado; Queries **no escriben jamás**.
- Queries pueden usar Dapper directamente (sin pasar por agregados).
- Commands siempre pasan por el agregado y respetan sus invariantes.

### 10.3. Eventos de dominio

Se publican tras `SaveChangesAsync` exitoso vía `DomainEventDispatcherInterceptor`.

Ejemplos:
- `HighIntensitySymptomDomainEvent` → análisis IA + alerta médico.
- `SubscriptionTrialExpiredDomainEvent` → notificación + bloqueo features premium.

---

## 11. Manejo de Errores

### 11.1. Flujo

1. Handlers retornan `Result.Failure(Error)` para errores esperados.
2. `BaseApiController` traduce `Result` → `ActionResult` según `ErrorType`.
3. `ExceptionHandlingMiddleware` captura excepciones técnicas → `ProblemDetails`.

### 11.2. ProblemDetails (RFC 7807)

```json
{
  "type": "https://neuroviva.api/errors/validation",
  "title": "Validation failed",
  "status": 400,
  "detail": "One or more validation errors occurred.",
  "instance": "/api/v1/patients",
  "traceId": "00-abc...",
  "errors": {
    "Name": ["Name is required."],
    "BirthDate": ["BirthDate cannot be in the future."]
  }
}
```

### 11.3. Catálogo de errores (extracto)

| Code | Type | Cuándo |
|------|------|--------|
| `patient.not_found` | NotFound | Paciente no existe o no pertenece al tenant. |
| `patient.not_assigned` | Forbidden | El usuario no tiene relación con el paciente. |
| `symptom.intensity_out_of_range` | Validation | Intensidad fuera de 1..10. |
| `subscription.trial_already_used` | Conflict | Tenant ya consumió trial. |
| `payment.method_required` | Validation | Falta método de pago activo. |
| `resource.invalid_state_transition` | Conflict | Transición inválida en aprobación. |
| `tenant.missing` | Unauthorized | JWT sin tenant_id. |

---

## 12. Configuración

### 12.1. appsettings.json (plantilla — sin valores reales)

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.EntityFrameworkCore": "Warning"
      }
    },
    "WriteTo": [
      { "Name": "Console" }
    ]
  },
  "Database": {
    "ConnectionString": "<<SET_VIA_USER_SECRETS_OR_ENV>>",
    "CommandTimeoutSeconds": 30,
    "EnableSensitiveDataLogging": false
  },
  "Supabase": {
    "Url": "<<https://<project>.supabase.co>>",
    "AnonKey": "<<set-in-secrets>>",
    "JwtIssuer": "<<https://<project>.supabase.co/auth/v1>>",
    "JwtAudience": "authenticated",
    "JwksUrl": "<<https://<project>.supabase.co/auth/v1/keys>>"
  },
  "Cors": {
    "AllowedOrigins": [ "https://app.neuroviva.com" ]
  },
  "RateLimiting": {
    "Default": { "PermitLimit": 100, "WindowSeconds": 60 },
    "Auth":    { "PermitLimit": 10,  "WindowSeconds": 60 },
    "Ai":      { "PermitLimit": 20,  "WindowSeconds": 3600 }
  },
  "Ai": {
    "Provider": "OpenAI",
    "OpenAI": { "ApiKey": "<<set-in-secrets>>", "Model": "gpt-4o-mini" },
    "Groq":   { "ApiKey": "<<set-in-secrets>>", "Model": "llama-3.1-70b-versatile" }
  },
  "Notifications": {
    "Push":  { "FirebaseCredentialsPath": "<<path-or-secret>>" },
    "Email": { "SmtpHost": "<<smtp.host>>", "SmtpPort": 587, "User": "<<>>", "Pass": "<<>>" },
    "Sms":   { "TwilioAccountSid": "<<>>", "TwilioAuthToken": "<<>>", "FromNumber": "<<>>" }
  },
  "Storage": {
    "Provider": "Supabase",
    "Bucket": "neuroviva-public"
  },
  "Payments": {
    "Provider": "Wompi",
    "Wompi": {
      "PublicKey": "<<>>",
      "PrivateKey": "<<set-in-secrets>>",
      "EventsKey": "<<set-in-secrets>>",
      "BaseUrl": "https://production.wompi.co/v1"
    }
  },
  "AllowedHosts": "*"
}
```

### 12.2. Reglas de secretos

- **Nunca** commitear secretos en `appsettings.Development.json`.
- Local: `dotnet user-secrets`.
- Producción: variables de entorno o secret manager del cloud provider.
- La app falla al arranque si faltan `Database:ConnectionString` o `Supabase:JwtIssuer`.

---

## 13. Módulos API — Endpoints

> Todos bajo `/api/v1`. JWT obligatorio salvo `[PUBLIC]`.

### 13.1. Auth & Onboarding

| Método | Ruta | Descripción | Roles |
|--------|------|-------------|-------|
| POST | `/auth/register` | Registro post-Supabase (crea perfil, asocia auth_user_id). | [PUBLIC] |
| POST | `/auth/select-role` | Selecciona rol post-registro. | Authenticated |
| GET  | `/auth/me` | Usuario actual + roles + tenant. | Authenticated |
| POST | `/onboarding/start` | Inicia onboarding según rol. | Authenticated |
| POST | `/onboarding/{sessionId}/answer` | Guarda respuesta de un paso. | Authenticated |
| POST | `/onboarding/{sessionId}/complete` | Completa onboarding. | Authenticated |
| GET  | `/onboarding/steps?role={role}` | Lista pasos para un rol. | Authenticated |
| GET  | `/preferences/me` | Preferencias del usuario actual. | Authenticated |
| PUT  | `/preferences/me` | Actualiza preferencias. | Authenticated |

### 13.2. Pacientes y relaciones

| Método | Ruta | Descripción | Roles |
|--------|------|-------------|-------|
| POST | `/patients` | Crea paciente. | Doctor, Caregiver, Admin |
| GET  | `/patients` | Lista pacientes (paginado). | Doctor, Caregiver, Admin |
| GET  | `/patients/{id}` | Detalle. | Asignados al paciente |
| PUT  | `/patients/{id}` | Actualiza. | Doctor, Admin |
| DELETE | `/patients/{id}` | Inactiva (soft delete). | Doctor, Admin |
| POST | `/patients/{id}/doctors` | Asigna médico. | Doctor, Admin |
| POST | `/patients/{id}/caregivers` | Asigna cuidador. | Doctor, Admin |
| GET  | `/patients/{id}/clinical-history` | Línea de tiempo clínica. | Asignados |
| POST | `/patients/{id}/clinical-history` | Agrega evento clínico. | Doctor, Caregiver |

### 13.3. Medicación

| Método | Ruta | Descripción | Roles |
|--------|------|-------------|-------|
| POST | `/patients/{id}/medications` | Crea medicamento. | Doctor, Caregiver |
| GET  | `/patients/{id}/medications` | Lista activos. | Asignados |
| PUT  | `/medications/{medId}` | Actualiza. | Doctor, Caregiver |
| DELETE | `/medications/{medId}` | Inactiva. | Doctor, Caregiver |
| POST | `/medications/{medId}/intakes` | Registra toma. | Caregiver, Patient |
| GET  | `/medications/{medId}/intakes` | Historial de tomas. | Asignados |

### 13.4. Salud (síntomas y ánimo)

| Método | Ruta | Descripción | Roles |
|--------|------|-------------|-------|
| POST | `/patients/{id}/symptoms` | Registra síntoma. | Caregiver, Patient |
| GET  | `/patients/{id}/symptoms` | Lista síntomas. | Asignados |
| POST | `/patients/{id}/moods` | Registra estado de ánimo. | Caregiver, Patient |
| GET  | `/patients/{id}/moods` | Lista estados. | Asignados |

### 13.5. Citas

| Método | Ruta | Descripción | Roles |
|--------|------|-------------|-------|
| POST | `/appointments` | Crea cita. | Doctor, Caregiver |
| GET  | `/appointments` | Lista (filtros). | Asignados |
| GET  | `/appointments/{id}` | Detalle. | Asignados |
| PATCH | `/appointments/{id}/confirm` | Confirma. | Doctor, Caregiver |
| PATCH | `/appointments/{id}/cancel` | Cancela. | Doctor, Caregiver |
| PATCH | `/appointments/{id}/complete` | Marca realizada. | Doctor |

### 13.6. IA y alertas

| Método | Ruta | Descripción | Roles |
|--------|------|-------------|-------|
| POST | `/patients/{id}/ai/analyze` | Análisis bajo demanda. | Doctor, Caregiver |
| GET  | `/patients/{id}/ai/analyses` | Historial de análisis. | Doctor, Caregiver |
| GET  | `/ai/analyses/{analysisId}` | Detalle. | Asignados |
| GET  | `/alerts` | Bandeja del médico. | Doctor |
| PATCH | `/alerts/{id}/seen` | Marca vista. | Doctor |
| PATCH | `/alerts/{id}/resolve` | Marca resuelta. | Doctor |
| GET  | `/notifications/me` | Notificaciones propias. | Authenticated |
| PATCH | `/notifications/{id}/read` | Marca leída. | Authenticated |

### 13.7. Recursos y comité científico

| Método | Ruta | Descripción | Roles |
|--------|------|-------------|-------|
| POST | `/resources` | Sube recurso (queda pendiente). | Authenticated |
| GET  | `/resources` | Lista pública (aprobados). | Authenticated |
| GET  | `/resources/{id}` | Detalle. | Authenticated |
| POST | `/resources/{id}/submit` | Envía a revisión. | Author |
| POST | `/resources/{id}/ai-review` | Dispara revisión IA. | Admin |
| POST | `/resources/{id}/committee-decision` | Aprueba o rechaza. | ScientificCommittee |
| GET  | `/resources/{id}/flow` | Trazabilidad del flujo. | Admin, Committee |
| POST | `/patients/{id}/resources/{resourceId}/progress` | Actualiza avance. | Caregiver, Patient |

### 13.8. Cursos

| Método | Ruta | Descripción | Roles |
|--------|------|-------------|-------|
| GET  | `/courses` | Lista por enfermedad. | Caregiver |
| GET  | `/courses/{id}` | Detalle. | Caregiver |
| POST | `/courses/{id}/progress` | Actualiza progreso. | Caregiver |
| GET  | `/courses/me/progress` | Mi progreso global. | Caregiver |

### 13.9. Grupos

| Método | Ruta | Descripción | Roles |
|--------|------|-------------|-------|
| POST | `/groups` | Crea grupo. | Authenticated |
| GET  | `/groups` | Lista públicos + mis grupos. | Authenticated |
| GET  | `/groups/{id}` | Detalle. | Miembro |
| POST | `/groups/{id}/join` | Se une. | Authenticated |
| POST | `/groups/{id}/leave` | Sale. | Miembro |
| POST | `/groups/{id}/messages` | Envía mensaje. | Miembro |
| GET  | `/groups/{id}/messages` | Lista (cursor). | Miembro |
| POST | `/groups/{id}/messages/{msgId}/reactions` | Reacciona. | Miembro |

### 13.10. Comunidad

| Método | Ruta | Descripción | Roles |
|--------|------|-------------|-------|
| POST | `/community/posts` | Crea post. | Authenticated |
| GET  | `/community/posts` | Feed por enfermedad. | Authenticated |
| GET  | `/community/posts/{id}` | Detalle. | Authenticated |
| POST | `/community/posts/{id}/comments` | Comenta. | Authenticated |
| POST | `/community/posts/{id}/reactions` | Reacciona. | Authenticated |
| DELETE | `/community/posts/{id}` | Elimina (autor o moderación). | Author/Admin |

### 13.11. Marketplace

| Método | Ruta | Descripción | Roles |
|--------|------|-------------|-------|
| POST | `/marketplace/stores` | Solicita alta. | Authenticated |
| GET  | `/marketplace/stores` | Lista aprobadas. | Authenticated |
| GET  | `/marketplace/stores/{id}` | Detalle. | Authenticated |
| POST | `/marketplace/stores/{id}/approve` | Aprueba. | Admin |
| POST | `/marketplace/stores/{id}/reject` | Rechaza. | Admin |
| POST | `/marketplace/stores/{id}/reports` | Reporta. | Authenticated |
| GET  | `/marketplace/reports` | Lista reportes. | Admin |

### 13.12. Suscripción y billing

| Método | Ruta | Descripción | Roles |
|--------|------|-------------|-------|
| GET  | `/billing/plans` | Lista planes activos. | Authenticated |
| GET  | `/billing/subscription` | Suscripción del tenant. | Admin |
| POST | `/billing/subscription/start-trial` | Inicia trial. | Admin |
| POST | `/billing/subscription/activate` | Activa (requiere método de pago). | Admin |
| POST | `/billing/subscription/cancel` | Cancela. | Admin |
| POST | `/billing/payment-methods` | Registra método. | Admin |
| GET  | `/billing/payment-methods` | Lista métodos. | Admin |
| PATCH | `/billing/payment-methods/{id}/default` | Establece predeterminado. | Admin |
| GET  | `/billing/charges` | Historial de cobros. | Admin |
| POST | `/billing/webhooks/wompi` | Webhook Wompi firmado. | [PUBLIC+Signature] |

### 13.13. Catálogo

| Método | Ruta | Descripción | Roles |
|--------|------|-------------|-------|
| GET  | `/catalog/diseases` | Lista enfermedades activas. | [PUBLIC] |
| GET  | `/catalog/diseases/{slug}` | Detalle. | [PUBLIC] |

### 13.14. Administración

| Método | Ruta | Descripción | Roles |
|--------|------|-------------|-------|
| GET  | `/admin/tenants` | Lista tenants. | Admin global |
| GET  | `/admin/users` | Lista usuarios cross-tenant. | Admin global |
| PATCH | `/admin/users/{id}/disable` | Inhabilita usuario. | Admin global |
| GET  | `/admin/kpis/overview` | KPIs generales. | Admin |
| GET  | `/admin/audit-log` | Bitácora de auditoría. | Admin |

---

## 14. Convenciones de Código

### 14.1. Idioma

- **Código (clases, métodos, variables, namespaces):** inglés.
- **Documentación y comentarios de arquitectura:** español.
- **Mensajes de error visibles al usuario:** español. El `Error.Code` siempre en inglés.

### 14.2. Nomenclatura

- Clases / records: `PascalCase`
- Métodos: `PascalCase`
- Parámetros y variables locales: `camelCase`
- Campos privados: `_camelCase`
- Interfaces: prefijo `I`
- Async: sufijo `Async`
- Folders → namespaces (1:1)

### 14.3. Reglas duras

- Prohibido `using NeuroViva.Infrastructure;` en `Application` o `Domain`.
- Prohibido `DbContext` en handlers.
- Prohibido `IQueryable` cruzando la frontera Application/Infrastructure.
- Prohibido lógica de negocio en controllers.
- Prohibido `throw new Exception(...)` para errores de negocio esperados — usar `Result`.
- Prohibido `string` para identificadores tipados — usar `Guid` o Value Objects.

---

## 15. Roadmap de Implementación

### Fase 0 — Fundaciones

1. Crear solución con los 4 proyectos y `Directory.Build.props`.
2. Configurar Serilog, ProblemDetails, versionado, Swagger, CORS.
3. Endpoint `/health`.

### Fase 1 — Autenticación y multitenancy

1. Validación JWT Supabase (JWKS).
2. `ICurrentUserService`, `ITenantContext`, `TenantResolutionMiddleware`.
3. EF Core con filtro global por tenant. Entidades `User` y `Tenant`.
4. Endpoint `GET /auth/me`.

### Fase 2 — Onboarding y catálogo

1. Entidades: `Disease`, `OnboardingStep`, `OnboardingSession`, `UserPreference`.
2. Endpoints de onboarding y preferencias.

### Fase 3 — Pacientes, cuidadores, médicos

1. Agregado `Patient` con relaciones y relaciones de acceso.
2. Historia clínica con paginación.

### Fase 4 — Medicación, síntomas, ánimo, citas

1. Aggregates con invariantes de dominio.
2. Eventos de dominio para síntomas de alta intensidad.

### Fase 5 — IA, alertas, notificaciones

1. `IAiAnalysisService` (provider configurable).
2. Dispatcher de notificaciones multicanal.
3. Bandeja de alertas para médicos.

### Fase 6 — Recursos y comité científico

1. Flujo de aprobación como máquina de estados.
2. Integración con revisión IA previa al comité.

### Fase 7 — Cursos, grupos, comunidad

1. Cursos con progreso.
2. Grupos con mensajería (REST, WebSocket en backlog).
3. Comunidad: posts, comentarios, reacciones.

### Fase 8 — Marketplace

1. Alta de tienda + flujo de aprobación.
2. Moderación y reportes.

### Fase 9 — Suscripción y billing

1. Trial, activación, métodos de pago.
2. Integración Wompi + webhook firmado.

### Fase 10 — Backoffice admin

1. Endpoints cross-tenant con políticas administrativas.
2. KPIs y bitácora de auditoría.

### Criterios de aceptación (cada fase)

- Tests unitarios en `Domain` y `Application` (objetivo 70% cobertura).
- Endpoints documentados en Swagger con `[ProducesResponseType]`.
- Sin warnings Roslyn.
- Review del arquitecto aprobado.
- Cambios de BD: script forward + rollback validado por DBA Expert.

---

## Apéndice A — Glosario

| Término | Definición |
|---------|------------|
| Tenant | Organización / espacio aislado de datos. |
| Aggregate | Conjunto de entidades con una raíz que protege invariantes. |
| Value Object | Tipo sin identidad, igualdad estructural, inmutable. |
| Domain Event | Hecho de negocio ocurrido dentro del Dominio. |
| CQRS | Separación entre comandos (escritura) y queries (lectura). |
| UoW | Unit of Work — gestiona la transacción y el `SaveChanges`. |
| RLS | Row Level Security — políticas de Supabase a nivel de fila. |

## Apéndice B — ADRs pendientes

- **ADR-001**: Selección definitiva de proveedor IA (OpenAI vs Groq vs híbrido).
- **ADR-002**: Estrategia de chat en grupos (REST vs SignalR vs Supabase Realtime).
- **ADR-003**: Estrategia de auditoría (append-only vs event log externo).
- **ADR-004**: Búsqueda en comunidad y recursos (PostgreSQL FTS vs Meilisearch).
- **ADR-005**: Estrategia de migraciones EF Core vs scripts SQL gestionados por DBA.
