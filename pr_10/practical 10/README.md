# Student Management REST API

A .NET 10 ASP.NET Core Web API for managing student records. It uses SQLite for persistent storage and exposes CRUD operations under `/api/students`.

## Run

```sh
dotnet run
```

Open `http://localhost:5080/` for the student management dashboard. The API uses port `5080` to avoid conflicts with services using port `5000`.

The dashboard supports searching, creating, editing, and deleting students through the REST API.

The database is created at `App_Data/students.db` on first launch. Set `ConnectionStrings:StudentDatabase` in `appsettings.json` to change its location.

## Endpoints

| Method | Route | Description |
| --- | --- | --- |
| `GET` | `/api/students` | List students; optionally filter with `?search=term` |
| `GET` | `/api/students/{id}` | Get a student by database ID |
| `POST` | `/api/students` | Create a student |
| `PUT` | `/api/students/{id}` | Replace a student's details |
| `DELETE` | `/api/students/{id}` | Delete a student |

Student number and email must be unique. Invalid input returns `400`, duplicate values return `409`, and missing IDs return `404`.

## Example

```sh
curl -X POST http://localhost:5080/api/students \
  -H 'Content-Type: application/json' \
  -d '{
    "studentNumber": "STU-1001",
    "firstName": "Avery",
    "lastName": "Morgan",
    "email": "avery.morgan@example.com",
    "department": "Computer Science",
    "dateOfBirth": "2004-05-12"
  }'
```

The API is organized into `Controllers`, `Data`, `Middleware`, and `Models`. `ApiExceptionMiddleware` logs unexpected failures and returns a generic Problem Details response; `RequestIdMiddleware` adds an `X-Request-Id` response header.
