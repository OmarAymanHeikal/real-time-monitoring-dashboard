# Real-Time System Monitoring Dashboard

Production-ready monitoring platform: simulated servers, metrics collection (Hangfire), anomaly alerts, report generation, and real-time updates (SignalR) to an Angular dashboard.

## Features

- **Backend**: ASP.NET Core 10, Clean Architecture, CQRS (MediatR), EF Core, SQL Server, Hangfire (recurring + fire-and-forget + delayed), SignalR, JWT, Swagger, health checks, Serilog.
- **Frontend**: Angular 17+, standalone components, lazy routes, Material, auth guards, JWT interceptor.

## Tech stack

.NET 10, EF Core 9, SQL Server, Hangfire, SignalR, Angular 17, Docker.

## Run locally

1. Backend: `dotnet restore` then `dotnet ef database update --project src/MonitoringDashboard.Infrastructure --startup-project src/MonitoringDashboard.WebAPI` then `dotnet run --project src/MonitoringDashboard.WebAPI`. API: http://localhost:5000. Swagger: /swagger. Hangfire: /hangfire (Admin).
2. Frontend: `cd client && npm install && npm start`. App: http://localhost:4200.
3. Login: admin / Admin@123 (seeded).

## Run with Docker

`docker-compose up -d`. API: 5000, Client: 4200.

## Docs

- ARCHITECTURE.md – layers, SOLID, schema, folder structure.
- DEPLOYMENT.md – env vars, DB, Docker, scaling, troubleshooting.
- API: Swagger at /swagger when API is running.

## Tests

Backend: `dotnet test tests/MonitoringDashboard.Tests`. Frontend: `cd client && npm test`.
