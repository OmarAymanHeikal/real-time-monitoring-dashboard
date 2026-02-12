# Architecture

## Clean Architecture

The solution follows Clean Architecture with four layers:

1. **Domain** (`MonitoringDashboard.Domain`)
   - Entities: Role, User, Server, Metric, Disk, Alert, Report
   - Enums: ServerStatus, AlertStatus, ReportStatus
   - Domain events (e.g. AlertTriggeredEvent, ReportCompletedEvent)
   - Exceptions (DomainException, NotFoundException)
   - Interfaces: IApplicationDbContext, ISystemMetricsService, ISystemMetricsSnapshot
   - No dependencies on other layers

2. **Application** (`MonitoringDashboard.Application`)
   - Use cases implemented as MediatR handlers (CQRS)
   - DTOs and request/response records
   - FluentValidation validators
   - Pipeline behavior: ValidationBehavior
   - Interfaces: IIdentityService, IJwtService, IHubNotificationService, IReportFileService
   - Depends only on Domain

3. **Infrastructure** (`MonitoringDashboard.Infrastructure`)
   - EF Core: ApplicationDbContext, entity configurations, migrations
   - Persistence: SeedData for roles, admin user, demo servers and metrics
   - Services: IdentityService, JwtService, SystemMetricsService, ReportFileService
   - Hangfire jobs: MetricsCollectionJob (recurring), ReportGenerationJob (fire-and-forget), AlertThresholdJob (recurring)
   - Implements Application and Domain interfaces
   - Depends on Application (and thus Domain)

4. **WebAPI** (`MonitoringDashboard.WebAPI`)
   - Controllers under `Api/V1`: Auth, Servers, Metrics, Alerts, Reports, Users
   - SignalR MonitoringHub and SignalRHubNotificationService (implements IHubNotificationService)
   - Middleware: ExceptionHandlingMiddleware
   - JWT and Hangfire dashboard configuration
   - Depends on Infrastructure

## SOLID

- **SRP**: Handlers do one use case; jobs do one job type; services have a single responsibility.
- **OCP**: New use cases added as new handlers/validators without changing existing ones; new jobs as new classes.
- **LSP**: Implementations (e.g. IdentityService, JwtService) substitute their interfaces without changing behavior.
- **ISP**: Small interfaces (IIdentityService, IJwtService, IHubNotificationService, IReportFileService, ISystemMetricsService).
- **DIP**: Application and WebAPI depend on abstractions (interfaces); Infrastructure and WebAPI provide implementations.

## Design patterns

- **CQRS**: Commands (Login, Register, CreateServer, ResolveAlert, RequestReport) and Queries (GetServer, ListServers, ListMetrics, ListAlerts, GetReport, ListReports, ListUsers, ListRoles) with MediatR.
- **Repository-style access**: IApplicationDbContext abstracts persistence; Infrastructure provides EF Core implementation.
- **Pipeline behavior**: ValidationBehavior runs FluentValidation before handlers.
- **Background jobs**: Recurring (metrics, alerts), fire-and-forget (report generation), with optional continuation/notification via IHubNotificationService.

## Database schema (conceptual)

- **Roles** (RoleId, Name, Description) – one-to-many **Users** (UserId, UserName, Email, PasswordHash, RoleId, RefreshToken, RefreshTokenExpiry, CreatedAt)
- **Servers** (ServerId, Name, IPAddress, Status, Description, CreatedAt) – one-to-many **Metrics**, **Disks**, **Alerts**, **Reports**
- **Metrics**: ServerId, CpuUsage, MemoryUsage, DiskUsage, ResponseTime, Status, Timestamp
- **Alerts**: ServerId, MetricType, MetricValue, Threshold, Status, CreatedAt, ResolvedAt
- **Reports**: ServerId, StartTime, EndTime, Status, FilePath, CreatedAt, CompletedAt

Indexes and relationships are defined in Fluent API under `Infrastructure/Persistence/Configurations`.

## Folder structure

- `src/MonitoringDashboard.Domain/` – Entities, Enums, Events, Exceptions, Interfaces
- `src/MonitoringDashboard.Application/` – Common (Behaviors, Exceptions, Interfaces, Models), Features (Auth, Servers, Metrics, Alerts, Reports, Users), DependencyInjection
- `src/MonitoringDashboard.Infrastructure/` – Persistence (DbContext, Configurations, SeedData), Services, BackgroundJobs, DependencyInjection
- `src/MonitoringDashboard.WebAPI/` – Controllers/Api/V1, Hubs, Middleware, Program.cs
- `tests/MonitoringDashboard.Tests/` – Unit and integration tests
- `client/` – Angular 17+ SPA (core, features, environments)
