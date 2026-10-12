using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MyPortfolio.Infrastructure.Data;
using MyPortfolio.Domain.Common;

namespace MyPortfolio.Infrastructure.Authentication;

public class IdentityDataSeeder : IIdentityDataSeeder
{
    private const string AdminRole = "Admin";
    private const string UserRole = "User";
    private readonly ApplicationDbContext _dbContext;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IConfiguration _configuration;
    private readonly ILogger<IdentityDataSeeder> _logger;

    public IdentityDataSeeder(
        ApplicationDbContext dbContext,
        RoleManager<IdentityRole> roleManager,
        UserManager<IdentityUser> userManager,
        IConfiguration configuration,
        ILogger<IdentityDataSeeder> logger)
    {
        _dbContext = dbContext;
        _roleManager = roleManager;
        _userManager = userManager;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        var strategy = _dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            // 1. Idempotently ensure the Admin role exists
            if (!await _roleManager.RoleExistsAsync(AdminRole))
            {
                var roleResult = await _roleManager.CreateAsync(new IdentityRole(AdminRole));
                if (!roleResult.Succeeded)
                {
                    var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Failed to create '{AdminRole}' role: {errors}");
                }
                _logger.LogInformation("Role '{Role}' created successfully.", AdminRole);
            }

            if (!await _roleManager.RoleExistsAsync(UserRole))
            {
                var roleResult = await _roleManager.CreateAsync(new IdentityRole(UserRole));
                if (!roleResult.Succeeded)
                {
                    var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Failed to create '{UserRole}' role: {errors}");
                }
                _logger.LogInformation("Role '{Role}' created successfully.", UserRole);
            }

            // 2. Read Admin credentials from configuration/environment/user-secrets (with safe fallback)
            var adminEmail = _configuration["Admin:Email"];
            if (string.IsNullOrWhiteSpace(adminEmail))
            {
                adminEmail = "admin@myportfolio.local";
                _logger.LogWarning("Admin:Email not specified in configuration; using default: {Email}", adminEmail);
            }

            var adminPassword = _configuration["Admin:Password"];
            if (string.IsNullOrWhiteSpace(adminPassword))
            {
                adminPassword = "Admin@MyPortfolio2026!";
                _logger.LogWarning("Admin:Password not specified in configuration; using default admin password.");
            }

            // 3. Idempotently check if Admin user exists
            var existingUser = await _userManager.FindByEmailAsync(adminEmail);
            if (existingUser == null)
            {
                var adminUser = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var userResult = await _userManager.CreateAsync(adminUser, adminPassword);
                if (!userResult.Succeeded)
                {
                    var errors = string.Join(", ", userResult.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Failed to create admin user '{adminEmail}': {errors}");
                }

                var addRoleResult = await _userManager.AddToRoleAsync(adminUser, AdminRole);
                if (!addRoleResult.Succeeded)
                {
                    var errors = string.Join(", ", addRoleResult.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Failed to assign '{AdminRole}' role to user '{adminEmail}': {errors}");
                }

                _logger.LogInformation("Admin user '{Email}' created and assigned to '{Role}' role.", adminEmail, AdminRole);
            }
            else
            {
                // If user already exists, preserve account and password, but ensure Admin role is assigned
                if (!await _userManager.IsInRoleAsync(existingUser, AdminRole))
                {
                    var addRoleResult = await _userManager.AddToRoleAsync(existingUser, AdminRole);
                    if (!addRoleResult.Succeeded)
                    {
                        var errors = string.Join(", ", addRoleResult.Errors.Select(e => e.Description));
                        throw new InvalidOperationException($"Failed to assign '{AdminRole}' role to existing user '{adminEmail}': {errors}");
                    }
                    _logger.LogInformation("Assigned existing user '{Email}' to '{Role}' role.", adminEmail, AdminRole);
                }
            }

            var seededAdmin = await _userManager.FindByEmailAsync(adminEmail)
                ?? throw new InvalidOperationException("Seeded administrator could not be loaded.");
            await AssignUnownedRecords(_dbContext.PortfolioSections, seededAdmin.Id);
            await AssignUnownedRecords(_dbContext.SkillCategories, seededAdmin.Id);
            await AssignUnownedRecords(_dbContext.Skills, seededAdmin.Id);
            await AssignUnownedRecords(_dbContext.Experiences, seededAdmin.Id);
            await AssignUnownedRecords(_dbContext.Projects, seededAdmin.Id);
            await AssignUnownedRecords(_dbContext.Educations, seededAdmin.Id);
            await AssignUnownedRecords(_dbContext.Certifications, seededAdmin.Id);
            await AssignUnownedRecords(_dbContext.SocialLinks, seededAdmin.Id);
            await AssignUnownedRecords(_dbContext.ContactMessages, seededAdmin.Id);
            var unownedProfiles = await _dbContext.UserProfiles.IgnoreQueryFilters()
                .Where(profile => profile.OwnerUserId == string.Empty).ToListAsync();
            foreach (var profile in unownedProfiles)
            {
                var owner = await _userManager.FindByIdAsync(profile.UserId);
                profile.AssignToUser(owner?.Id ?? seededAdmin.Id);
            }
            await AssignUnownedRecords(_dbContext.PortfolioAnalyticsEvents, seededAdmin.Id);
            await AssignUnownedRecords(_dbContext.PortfolioSiteSettings, seededAdmin.Id);
            await _dbContext.SaveChangesAsync();
        });
    }

    private async Task AssignUnownedRecords<TEntity>(DbSet<TEntity> records, string ownerId)
        where TEntity : BaseEntity
    {
        var unowned = await records.IgnoreQueryFilters().Where(record => record.OwnerUserId == string.Empty).ToListAsync();
        foreach (var record in unowned) record.AssignOwner(ownerId);
    }
}
