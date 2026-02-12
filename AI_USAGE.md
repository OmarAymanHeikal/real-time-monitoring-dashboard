# AI tools usage

## How AI was used

- **Architecture and layout**: Solution structure (Clean Architecture layers, project references), folder layout for Domain/Application/Infrastructure/WebAPI and Angular client were designed with AI assistance and then adjusted to match the specification.
- **Scaffolding**: Entity classes, DTOs, MediatR commands/queries and handlers, validators, EF configurations, and API controllers were initially generated and then refined for naming, consistency, and error handling.
- **Documentation**: README, ARCHITECTURE, DEPLOYMENT, and API documentation were drafted with AI and edited for accuracy and completeness.
- **Boilerplate**: Middleware, JWT setup, SignalR hub and notification service, Hangfire job registration, and Angular routes/guards/interceptors were produced with AI and reviewed.

## Review and quality

- All generated code was reviewed for correctness, security (e.g. password hashing, JWT handling), and alignment with SOLID and Clean Architecture.
- Manual changes were made where needed: dependency directions, exception types, validation behavior, and API response shapes.
- Tests (unit and integration) were added and adjusted manually to match the actual handlers and API behavior.

## Summary

AI was used as a productivity aid for structure, boilerplate, and documentation. Final design decisions, security-sensitive code, and test coverage remain under human review and control.
