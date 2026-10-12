using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using MyPortfolio.API.Middlewares;
using MyPortfolio.Application;
using MyPortfolio.Infrastructure;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting MyPortfolio API host...");

    var builder = WebApplication.CreateBuilder(args);

    // 1. Serilog Setup
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File(
            path: "logs/myportfolio-.log",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 14));

    // 2. Add Layers (Clean Architecture DI)
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    // 3. Exception Handling & ProblemDetails
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();

    // 4. JWT Authentication Foundation
    var jwtKey = builder.Configuration["Jwt:Key"] ?? "MyPortfolio_SuperSecret_Jwt_SigningKey_For_Production_Ready_Auth_2026_Key!";
    var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "MyPortfolioAPI";
    var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "MyPortfolioClient";

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            RoleClaimType = System.Security.Claims.ClaimTypes.Role,
            NameClaimType = System.Security.Claims.ClaimTypes.NameIdentifier
        };
    });

    builder.Services.AddAuthorization();

    // 5. CORS for Angular client
    var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
        ?? new[] { "http://localhost:4200" };

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAngularClient", policy =>
        {
            policy.SetIsOriginAllowed(origin =>
                  {
                      if (string.IsNullOrEmpty(origin)) return false;
                      if (allowedOrigins.Contains("*") || allowedOrigins.Contains(origin)) return true;
                      // Allow localhost & monsterasp subdomains by default
                      if (origin.Contains("localhost") || origin.Contains("monsterasp.net")) return true;
                      return false;
                  })
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
    });

    // 6. Controllers & Swagger/OpenAPI
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "MyPortfolio API",
            Version = "v1",
            Description = "Standard ASP.NET Core (.NET 10) Clean Architecture Web API"
        });

        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Enter your JWT token in the format: Bearer {your token}"
        });

        c.AddSecurityRequirement(_ => new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecuritySchemeReference("Bearer"),
                new List<string>()
            }
        });
    });

    var app = builder.Build();

    // Pipeline Configuration
    app.UseExceptionHandler();

    app.UseSerilogRequestLogging();

    //if (app.Environment.IsDevelopment() || true)
    //{ }
    app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "MyPortfolio API v1");
            c.RoutePrefix = "swagger";
        });
    

    app.UseCors("AllowAngularClient");

    // Security Headers & Document Access Hardening Middleware
    app.Use(async (context, next) =>
    {
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Append("X-Frame-Options", "SAMEORIGIN");
        context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
        context.Response.Headers.Append("Permissions-Policy", "geolocation=(), microphone=(), camera=()");

        // Block direct unmediated static downloads of uploaded CVs
        if (context.Request.Path.StartsWithSegments("/uploads/cv", StringComparison.OrdinalIgnoreCase))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync("{\"succeeded\":false,\"message\":\"Direct access to uploaded documents is forbidden. Please use /api/portfolio/cv-download.\"}");
            return;
        }

        await next();
    });

    app.UseStaticFiles();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    // 7. Auto-migrate database & seed initial roles/data
    using (var scope = app.Services.CreateScope())
    {
        try
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<MyPortfolio.Infrastructure.Data.ApplicationDbContext>();
            await dbContext.Database.MigrateAsync();
            Log.Information("Database migrations applied successfully.");

            var seeder = scope.ServiceProvider.GetRequiredService<MyPortfolio.Infrastructure.Authentication.IIdentityDataSeeder>();
            await seeder.SeedAsync();
            Log.Information("Identity data seeded successfully.");

            var guestSeeder = scope.ServiceProvider.GetRequiredService<MyPortfolio.Infrastructure.Data.IGuestPortfolioDataSeeder>();
            await guestSeeder.SeedGuestPortfolioAsync();
            Log.Information("Guest portfolio data seeded successfully.");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred during database migration or data seeding. The application will continue running.");
        }
    }

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
