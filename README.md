# Job Tracker API

A RESTful Web API for tracking job applications, built with ASP.NET Core, Entity Framework Core and SQLite.

I built this project as a hands-on way to learn backend development with .NET. I'm a frontend developer (React, Next.js, TypeScript), so I'm designing the API to be easy to consume from a modern frontend, and I'll build the UI for it myself in a separate step.

## Features

- Full CRUD for job applications (company, position, status, applied date, notes)
- Four application statuses: `Applied`, `Interview`, `Rejected`, `Offer`
- Filter by status and search by company name or position
- Summary endpoint with the number of applications per status
- Dedicated endpoint for quick status updates
- DTOs that separate the API contract from the database model
- Request validation with data annotations
- CORS configured for local frontend development
- OpenAPI document available in development

## Tech Stack

- C# / ASP.NET Core Web API (controllers)
- Entity Framework Core with SQLite
- OpenAPI

## Project Structure

```
job-tracker-api/
├── JobTracker.sln
└── src/
    └── JobTracker/
        ├── Controllers/    # API endpoints
        ├── Data/           # DbContext
        ├── Dtos/           # Request and response models
        ├── Models/         # Database entities and enums
        ├── Migrations/     # EF Core migrations
        ├── appsettings.json
        └── Program.cs
```

## Getting Started

### Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) (I'm developing with .NET 11)
- The EF Core CLI tool:

```bash
dotnet tool install --global dotnet-ef
```

### Run locally

```bash
git clone https://github.com/<your-username>/job-tracker-api.git
cd job-tracker-api/src/JobTracker

# Create the SQLite database from the migrations
dotnet ef database update

# Start the API
dotnet run
```

The console output shows the URL the API is listening on (for example `http://localhost:5000`). The SQLite database file `jobtracker.db` is created in the project folder.

## API Endpoints

Base path: `/api/jobapplications`

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/jobapplications` | List applications (newest first). Optional query params: `status`, `search` |
| GET | `/api/jobapplications/{id}` | Get a single application |
| GET | `/api/jobapplications/summary` | Count of applications per status |
| POST | `/api/jobapplications` | Create an application |
| PUT | `/api/jobapplications/{id}` | Replace an application |
| PATCH | `/api/jobapplications/{id}/status` | Update only the status |
| DELETE | `/api/jobapplications/{id}` | Delete an application |

### Examples

Create an application:

```http
POST /api/jobapplications
Content-Type: application/json

{
  "companyName": "Example Tech",
  "position": "Frontend Developer",
  "status": "Applied",
  "notes": "Applied via LinkedIn"
}
```

Response:

```json
{
  "id": 1,
  "companyName": "Example Tech",
  "position": "Frontend Developer",
  "status": "Applied",
  "appliedDate": "2026-11-16T12:00:00Z",
  "notes": "Applied via LinkedIn"
}
```

Filter and search:

```http
GET /api/jobapplications?status=Interview
GET /api/jobapplications?search=frontend
GET /api/jobapplications?status=Applied&search=tech
```

Update only the status:

```http
PATCH /api/jobapplications/1/status
Content-Type: application/json

{ "status": "Interview" }
```

Summary:

```http
GET /api/jobapplications/summary
```

```json
{
  "Applied": 5,
  "Interview": 2,
  "Rejected": 1,
  "Offer": 0
}
```

Invalid requests (for example a missing `companyName` or an unknown status value) return `400 Bad Request`.

## Frontend Integration

CORS is enabled for `http://localhost:3000` and `http://localhost:5173`, so a React or Next.js dev server can call the API directly. Enums are serialized as strings (`"Interview"` instead of `1`), which maps nicely to a TypeScript union type:

```ts
type ApplicationStatus = "Applied" | "Interview" | "Rejected" | "Offer";
```

## Roadmap

- [ ] Pagination and sorting
- [ ] Centralized error handling
- [ ] Unit and integration tests
- [ ] React / Next.js frontend

## What I'm Learning

- Structuring a .NET Web API (controllers, DbContext, DTOs)
- Dependency injection
- Entity Framework Core migrations and LINQ queries (filtering, projection, grouping)
- Designing REST endpoints that are pleasant to use from a frontend
