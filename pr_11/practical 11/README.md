# Smart Campus Service Portal

An ASP.NET Core MVC application that connects a responsive campus service desk to a SQLite database and a REST API. The MVC portal and API use the same request repository and database.

## Features

- Campus dashboard with request totals and recent activity
- Submit service requests with category, campus location, priority, and contact details
- Search requests by keyword and filter by status
- View request details and update request status
- REST API for listing, filtering, creating, updating status, and deleting requests
- SQLite schema initialization at application startup

## Run

```bash
dotnet restore
dotnet run
```

The database file (`smart-campus.db`) is created in the application directory on first run.

## API

Base route: `/api/requests`

| Method | Route | Description |
|---|---|---|
| GET | `/api/requests?search=...&status=...` | List and filter requests |
| GET | `/api/requests/{id}` | Get one request |
| POST | `/api/requests` | Submit a request |
| PATCH | `/api/requests/{id}/status` | Change status |
| DELETE | `/api/requests/{id}` | Delete a request |

Example request body for `POST /api/requests`:

```json
{
  "title": "Projector not working",
  "description": "The projector does not power on before the afternoon lecture.",
  "category": "IT Support",
  "location": "Science Building, Room 204",
  "priority": "High",
  "requesterName": "Alex Student",
  "requesterEmail": "alex@campus.edu"
}
```

Valid statuses are `Submitted`, `In Progress`, `Resolved`, and `Cancelled`. The status update endpoint accepts `{"status":"In Progress"}`.
