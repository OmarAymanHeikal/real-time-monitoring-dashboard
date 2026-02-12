# Deployment

## Prerequisites

- .NET 9 SDK (build) or .NET 9 runtime (run)
- SQL Server (or LocalDB for dev)
- Node.js 20+ (for building Angular client)
- Docker and Docker Compose (optional)

## Environment variables

### API (MonitoringDashboard.WebAPI)

| Variable | Description | Example |
|----------|-------------|---------|
| ConnectionStrings__DefaultConnection | SQL Server connection string | Server=.;Database=MonitoringDashboard;Trusted_Connection=True; |
| Jwt__Key | Secret key for JWT (min 32 chars) | YourSecretKey... |
| Jwt__Issuer | JWT issuer | MonitoringDashboard |
| Jwt__Audience | JWT audience | MonitoringDashboard |
| Jwt__ExpiryMinutes | Access token lifetime | 60 |
| Reports__BasePath | Directory for report files | /app/reports |

### Client

- For production, set API base URL (e.g. via environment or build substitution) so the client calls the correct API.

## Database setup

1. Create the database (or use existing SQL Server instance).
2. Run EF Core migrations:
   ```bash
   dotnet ef database update --project src/MonitoringDashboard.Infrastructure --startup-project src/MonitoringDashboard.WebAPI
   ```
3. Seed data (roles, admin user, demo servers) runs automatically on first API startup via `SeedAndScheduleJobsAsync`.

## Docker

- **Build and run with docker-compose**
  ```bash
  docker-compose up -d
  ```
  This starts SQL Server, the API, and the Angular client (nginx). API uses the `db` service for the connection string.

- **Build API image only**
  From repo root:
  ```bash
  docker build -f Dockerfile.api -t monitoring-api .
  ```
  Run with a connection string to your SQL Server.

## Cloud deployment

- **API**: Deploy as a container or to Azure App Service / AWS / GCP. Set connection string and JWT settings via environment variables or key vault.
- **Database**: Use managed SQL (e.g. Azure SQL, RDS). Run migrations as part of release or a one-off job.
- **Hangfire**: Uses the same SQL database; for multiple API instances, all point to the same DB so job storage is shared.
- **SignalR**: For multiple API instances, add a backplane (e.g. Redis) and configure SignalR to use it.
- **Client**: Build with `npm run build` and serve from static hosting or a CDN; set API URL for production.

## Scaling

- Scale API instances behind a load balancer; use the same SQL for Hangfire and the same (or shared) report file store if needed.
- Use Redis (or similar) as SignalR backplane for real-time messages across instances.
- Consider read replicas for heavy metric/alert read traffic.

## Monitoring and logging

- Serilog is configured for structured logs; add sinks (e.g. Seq, Application Insights) via configuration.
- Health check: `GET /health` (includes DB check when using AddDbContextCheck).
- Hangfire dashboard at `/hangfire` for job monitoring (restrict to Admin role in production).

## Troubleshooting

- **DB connection failed**: Check connection string, firewall, and that SQL Server is running. For Docker, ensure the `db` service is healthy before the API starts.
- **Hangfire jobs not running**: Confirm Hangfire is using the same connection string and that the Hangfire server is started (AddHangfireServer).
- **SignalR not connecting**: Ensure CORS allows the client origin and that JWT is passed (e.g. via query `access_token` for WebSockets if required).
- **401 on API**: Verify JWT key, issuer, and audience match between API and token generation; check token expiry.
