# MyPortfolio - Clean Architecture Backend (.NET 10)

Standard ASP.NET Core backend built using Clean Architecture principles, Entity Framework Core, SQL Server, and ASP.NET Core Identity with JWT Authentication.

## Solution Architecture

- **`src/MyPortfolio.Domain`**: Core domain models, enums, value objects, domain rules, and abstractions.
- **`src/MyPortfolio.Application`**: Use cases, CQRS commands/queries via MediatR, FluentValidation validators, and application interfaces.
- **`src/MyPortfolio.Infrastructure`**: EF Core `ApplicationDbContext`, SQL Server configuration, ASP.NET Core Identity stores, external services, and file storage.
- **`src/MyPortfolio.API`**: REST controllers, JWT Bearer authentication, middleware, Swagger/OpenAPI, Serilog logging, and dependency injection wiring.

## Tech Stack

- .NET 10
- ASP.NET Core Web API
- Entity Framework Core
- SQL Server
- ASP.NET Core Identity
- JWT Bearer Authentication
- MediatR (CQRS)
- FluentValidation
- Serilog (Console & Rolling File)
- Swagger / OpenAPI

## Running Locally

```powershell
cd "d:\my projects\MyPortfolio\MyPortfolio\backend"
dotnet run --project src/MyPortfolio.API/MyPortfolio.API.csproj
```

- **Swagger UI**: `http://localhost:5174/swagger`
- **API Health**: `http://localhost:5174/api/health`
- **Database Connection Test**: `http://localhost:5174/api/health/db`
