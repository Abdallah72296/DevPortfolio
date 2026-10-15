# MyPortfolio - Clean Architecture Backend (.NET 10)

Standard ASP.NET Core backend built using Clean Architecture principles, Entity Framework Core, SQL Server, and ASP.NET Core Identity with JWT Authentication.

## Solution Architecture

- **`src/MyPortfolio.Domain`**: Core domain models, enums, value objects, domain rules, and abstractions.
- **`src/MyPortfolio.Application`**: Use cases, CQRS commands/queries via MediatR, FluentValidation validators, and application interfaces.
- **`src/MyPortfolio.Infrastructure`**: EF Core `ApplicationDbContext`, SQL Server configuration, ASP.NET Core Identity stores, external services, email delivery, and file storage.
- **`src/MyPortfolio.API`**: REST controllers, JWT Bearer authentication, middleware, rate limiting, Swagger/OpenAPI, Serilog logging, and dependency injection wiring.
- **`tests/MyPortfolio.IntegrationTests`**: Automated integration, multi-user resolution, and authorization test suite.

## Tech Stack

- .NET 10
- ASP.NET Core Web API
- Entity Framework Core
- Microsoft SQL Server
- ASP.NET Core Identity
- JWT Bearer Authentication
- MediatR (CQRS)
- FluentValidation
- ASP.NET Core Rate Limiting
- Serilog (Console & Rolling File)
- Swagger / OpenAPI

## Running Locally

```powershell
dotnet run --project src/MyPortfolio.API/MyPortfolio.API.csproj
```

- **Swagger UI**: Available at `/swagger`
- **API Health**: Available at `/api/health`
- **Database Connection Test**: Available at `/api/health/db`
