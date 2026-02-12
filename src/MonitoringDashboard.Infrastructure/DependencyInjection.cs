using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MonitoringDashboard.Domain.Interfaces;
using MonitoringDashboard.Application.Common.Interfaces;
using MonitoringDashboard.Infrastructure.Persistence;
using MonitoringDashboard.Infrastructure.Services;
using Hangfire;
using Hangfire.SqlServer;

namespace MonitoringDashboard.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var conn = configuration.GetConnectionString("DefaultConnection") ?? "Server=(localdb)\\mssqllocaldb;Database=MonitoringDashboard;Trusted_Connection=True;MultipleActiveResultSets=true";
        services.AddDbContext<ApplicationDbContext>(opts => opts.UseSqlServer(conn));
        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IReportFileService, ReportFileService>();
        services.AddScoped<IEmailService, SmtpEmailService>();
        services.AddSingleton<ISystemMetricsService, SystemMetricsService>();

        // Redis Cache - falls back to in-memory if Redis not available
        var redisConnection = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrEmpty(redisConnection))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnection;
                options.InstanceName = "MonitoringDashboard_";
            });
            
            services.AddSingleton<StackExchange.Redis.IConnectionMultiplexer>(sp =>
                StackExchange.Redis.ConnectionMultiplexer.Connect(redisConnection));
            
            services.AddScoped<ICacheService, RedisCacheService>();
        }
        else
        {
            services.AddDistributedMemoryCache();
            services.AddScoped<ICacheService, RedisCacheService>();
        }

        services.AddHangfire(c => c.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSqlServerStorage(conn, new SqlServerStorageOptions { CommandBatchMaxTimeout = TimeSpan.FromMinutes(5) }));
        services.AddHangfireServer();

        services.AddScoped<BackgroundJobs.MetricsCollectionJob>();
        services.AddScoped<BackgroundJobs.ReportGenerationJob>();
        services.AddScoped<BackgroundJobs.AlertThresholdJob>();
        services.AddScoped<BackgroundJobs.MaintenanceNotificationJob>();
        services.AddScoped<BackgroundJobs.ReportCleanupJob>();

        return services;
    }

    public static async Task SeedAndScheduleJobsAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var identity = scope.ServiceProvider.GetRequiredService<IIdentityService>();
        await SeedData.SeedAsync(db, identity);

        var jobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
        jobManager.AddOrUpdate<BackgroundJobs.MetricsCollectionJob>("metrics-collection", j => j.ExecuteAsync(), "*/2 * * * *");
        jobManager.AddOrUpdate<BackgroundJobs.AlertThresholdJob>("alert-threshold", j => j.ExecuteAsync(), "*/5 * * * *");
    }
}
