using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyPortfolio.Application.Common.Interfaces;
using MyPortfolio.Infrastructure.Data;

namespace MyPortfolio.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        // Register EF Core DbContext with SQL Server
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorNumbersToAdd: null);
            }));

        // Register ASP.NET Core Identity Foundation
        services.AddIdentity<IdentityUser, IdentityRole>(options =>
        {
            // Password policy suitable for modern secure application
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 8;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;

            // User policy
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        // Register IApplicationDbContext abstraction for Application layer
        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

        // Register Generic Repository pattern
        services.AddScoped(typeof(IGenericRepository<>), typeof(Data.Repositories.GenericRepository<>));

        // Register Authentication & Token services
        services.AddScoped<Authentication.ITokenService, Authentication.TokenService>();
        services.AddScoped<IIdentityService, Authentication.IdentityService>();
        services.AddScoped<Authentication.IIdentityDataSeeder, Authentication.IdentityDataSeeder>();
        services.AddScoped<Data.IGuestPortfolioDataSeeder, Data.GuestPortfolioDataSeeder>();

        // Register CV Parsing and AI Options
        services.Configure<CV.AiOptions>(configuration.GetSection(CV.AiOptions.SectionName));
        services.AddScoped<Application.CV.Interfaces.ICvParser, CV.GeminiCvParser>();

        // Register File Upload Service
        services.AddScoped<IFileUploadService, Files.FileUploadService>();

        // Register User Data Cleanup Service
        services.AddScoped<IUserDataCleanupService, Data.UserDataCleanupService>();

        return services;
    }
}
