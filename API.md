# API and SignalR

## Base URL

- Local: `http://localhost:5000`
- API version prefix: `/api/v1`

## Authentication

- **Login** `POST /api/v1/auth/login`  
  Body: `{ "userName": "admin", "password": "Admin@123" }`  
  Response: `{ "accessToken", "refreshToken", "role" }`

- **Refresh** `POST /api/v1/auth/refresh`  
  Body: `{ "refreshToken": "..." }`  
  Response: `{ "accessToken", "newRefreshToken" }`

- **Register** `POST /api/v1/auth/register`  
  Body: `{ "userName", "email", "password", "roleId" }`  
  Response: `{ "userId" }`

Use the `accessToken` in the header: `Authorization: Bearer <token>`.

## Servers

- `GET /api/v1/servers` – list (query: search, status, page, pageSize)
- `GET /api/v1/servers/{id}` – get one
- `POST /api/v1/servers` – create (body: name, ipAddress?, description?)
- `PUT /api/v1/servers/{id}` – update (body: name, ipAddress?, description?, status?)
- `DELETE /api/v1/servers/{id}` – delete

## Metrics

- `GET /api/v1/metrics/server/{serverId}` – list (query: from?, to?, page, pageSize)

## Alerts

- `GET /api/v1/alerts` – list (query: serverId?, status?, page, pageSize)
- `POST /api/v1/alerts/{id}/resolve` – mark resolved

## Reports

- `GET /api/v1/reports` – list (query: serverId?, status?, page, pageSize)
- `GET /api/v1/reports/{id}` – get one
- `POST /api/v1/reports/request` – request generation (body: serverId, startTime, endTime); returns reportId, enqueues job
- `GET /api/v1/reports/{id}/download` – download file (if completed)

## Users (Admin)

- `GET /api/v1/users` – list (query: page, pageSize)
- `GET /api/v1/users/roles` – list roles

## SignalR hub

- **Endpoint**: `/hubs/monitoring`  
  Connect with JWT: pass token as query `access_token` or in header (depending on client).

- **Client methods**
  - `SubscribeToServer(serverId)` – join group for server metrics
  - `UnsubscribeFromServer(serverId)`
  - `JoinAlerts()` – receive alert notifications
  - `JoinReports()` – receive report status

- **Server events**
  - `MetricUpdate` – payload: { serverId, cpuUsage, memoryUsage, diskUsage, responseTime, status, timestamp }
  - `Alert` – payload: { alertId, serverId, metricType, metricValue, threshold, status, createdAt }
  - `ReportStatus` – payload: { reportId, status, filePath }

## Validation errors

On 400, body shape: `{ "title": "Validation failed", "errors": [ { "propertyName", "errorMessage" } ] }`.
