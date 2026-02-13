using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MonitoringDashboard.Application;
using MonitoringDashboard.Infrastructure;
using MonitoringDashboard.Application.Common.Interfaces;
using MonitoringDashboard.WebAPI.Hubs;
using MonitoringDashboard.WebAPI.Middleware;
using Serilog;
using System.Text;
using Hangfire;
using AspNetCoreRateLimit;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, lc) => lc.ReadFrom.Configuration(ctx.Configuration).Enrich.FromLogContext().WriteTo.Console());

// Rate Limiting
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.Configure<IpRateLimitPolicies>(builder.Configuration.GetSection("IpRateLimitPolicies"));
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

// API Versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new HeaderApiVersionReader("X-Api-Version"));
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Monitoring Dashboard API", Version = "v1", Description = "Real-Time System Monitoring Dashboard API" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme { Type = SecuritySchemeType.Http, Scheme = "bearer", BearerFormat = "JWT", In = ParameterLocation.Header, Description = "JWT" });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement { { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, Array.Empty<string>() } });
});

builder.Services.AddCors(o => o.AddDefaultPolicy(p => p
    .WithOrigins("http://localhost:4200", "http://localhost:5000")
    .AllowAnyMethod()
    .AllowAnyHeader()
    .AllowCredentials()));

var key = builder.Configuration["Jwt:Key"] ?? "MonitoringDashboard-SuperSecretKey-AtLeast32Chars!";
var keyBytes = Encoding.UTF8.GetBytes(key);
if (keyBytes.Length < 32) Array.Resize(ref keyBytes, 32);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "MonitoringDashboard",
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "MonitoringDashboard",
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
        o.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                var path = ctx.Request.Path;
                if (path.StartsWithSegments("/hubs")) ctx.Token = ctx.Request.Query["access_token"];
                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization(o => o.AddPolicy("Admin", p => p.RequireRole("Admin")));

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddSignalR();
builder.Services.AddHealthChecks().AddDbContextCheck<MonitoringDashboard.Infrastructure.Persistence.ApplicationDbContext>("database");

builder.Services.AddScoped<IHubNotificationService, SignalRHubNotificationService>();

var app = builder.Build();

await app.Services.SeedAndScheduleJobsAsync();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<RequestResponseLoggingMiddleware>();
app.UseIpRateLimiting();
app.UseSerilogRequestLogging();

// Routing must be before CORS for SignalR
app.UseRouting();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

// Map endpoints
app.MapControllers();
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new MonitoringDashboard.WebAPI.Middleware.HangfireAuthorizationFilter(app.Configuration) },
    AppPath = app.Environment.IsDevelopment() ? "http://localhost:4200" : app.Configuration["Frontend:Url"] ?? "/"
});
app.MapHub<MonitoringHub>("/hubs/monitoring");
app.MapHealthChecks("/health");

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1"));

app.Run();

// Make Program class accessible for integration tests
public partial class Program { }
