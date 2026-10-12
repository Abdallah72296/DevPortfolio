using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyPortfolio.Domain.Entities;

namespace MyPortfolio.Infrastructure.Data;

public class GuestPortfolioDataSeeder : IGuestPortfolioDataSeeder
{
    private const string GuestRole = "Guest";
    private const string GuestEmail = "guest@myportfolio.local";
    private const string GuestPassword = "Guest@Portfolio2026!";

    private readonly ApplicationDbContext _dbContext;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ILogger<GuestPortfolioDataSeeder> _logger;

    public GuestPortfolioDataSeeder(
        ApplicationDbContext dbContext,
        RoleManager<IdentityRole> roleManager,
        UserManager<IdentityUser> userManager,
        ILogger<GuestPortfolioDataSeeder> logger)
    {
        _dbContext = dbContext;
        _roleManager = roleManager;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task SeedGuestPortfolioAsync()
    {
        var strategy = _dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            // 1. Ensure Guest Role exists
            if (!await _roleManager.RoleExistsAsync(GuestRole))
            {
                await _roleManager.CreateAsync(new IdentityRole(GuestRole));
                _logger.LogInformation("Role '{Role}' created successfully.", GuestRole);
            }

            // 2. Ensure Guest User exists
            var guestUser = await _userManager.FindByEmailAsync(GuestEmail);
            if (guestUser == null)
            {
                guestUser = new IdentityUser
                {
                    UserName = "guest",
                    Email = GuestEmail,
                    EmailConfirmed = true
                };

                var createResult = await _userManager.CreateAsync(guestUser, GuestPassword);
                if (createResult.Succeeded)
                {
                    await _userManager.AddToRoleAsync(guestUser, GuestRole);
                    _logger.LogInformation("Guest user '{Email}' created successfully.", GuestEmail);
                }
                else
                {
                    _logger.LogWarning("Failed to create guest user: {Errors}",
                        string.Join(", ", createResult.Errors.Select(e => e.Description)));
                }
            }

            // 2.5 Seed UserProfile tied to guest user if empty
            if (!await _dbContext.UserProfiles.AnyAsync())
            {
                var paragraphs = new List<string>
                {
                    ".NET Backend Developer with professional and freelance experience building robust enterprise backend systems using C#, ASP.NET Core Web API, EF Core, Clean Architecture, CQRS, and RabbitMQ.",
                    "Experienced in financial, banking, project management, and business systems, with hands-on experience in RESTful API development, external banking integrations, asynchronous workflows, authentication, and relational databases.",
                    "Proven track record delivering mission-critical applications at Jedra and international freelance clients, including RabbitMQ-based pipelines and financial transaction aggregation engines. Military Service: Completed."
                };

                var profile = new UserProfile(
                    userId: guestUser?.Id ?? "guest",
                    fullName: "Abdallah Abdeltawab",
                    title: ".NET Backend Developer",
                    headlineRole: ".NET Backend Developer",
                    badge: ".NET BACKEND DEVELOPER • CLEAN ARCHITECTURE",
                    location: "Giza, Egypt (Remote & Worldwide)",
                    email: "abdallahabdeltawab6@gmail.com",
                    phone: "+20 1002215065",
                    summary: ".NET Backend Developer with professional and freelance experience building backend systems using C#, ASP.NET Core Web API, EF Core, Clean Architecture, CQRS, and RabbitMQ.",
                    longBio: "I am a passionate .NET Backend Developer with professional and freelance experience building robust enterprise backend architectures. With deep expertise in C#, ASP.NET Core Web API, EF Core, Clean Architecture, CQRS, and RabbitMQ, I specialize in financial transaction processing, external banking integrations, asynchronous workflows, and high-performance relational databases. Military Service: Completed.",
                    avatarUrl: "assets/images/developer_avatar.jpg",
                    cvUrl: "#contact",
                    yearsOfExperience: "3+",
                    paragraphsJson: System.Text.Json.JsonSerializer.Serialize(paragraphs),
                    availableForHire: true,
                    cardCaption: ".NET Backend Developer • Giza, Egypt"
                );

                await _dbContext.UserProfiles.AddAsync(profile);
                _logger.LogInformation("Seeded UserProfile for guest into database.");
            }

            // 3. Seed Experiences from real CV if empty
            if (!await _dbContext.Experiences.AnyAsync())
            {
                var exp1 = new Experience(
                    jobTitle: "Backend Developer (.NET)",
                    companyName: "Jedra",
                    description: "Engineered backend services for financial transaction analysis, integrating multiple external banking APIs and automating transaction processing workflows with ASP.NET Core Web API. Implemented an asynchronous RabbitMQ-based pipeline that decoupled transaction processing from the main API flow, improving reliability and scalability under load. Designed and implemented 20+ RESTful APIs using Clean Architecture and CQRS.",
                    startDate: new DateTime(2024, 12, 1, 0, 0, 0, DateTimeKind.Utc),
                    endDate: new DateTime(2025, 12, 1, 0, 0, 0, DateTimeKind.Utc),
                    isCurrent: true,
                    displayOrder: 1,
                    isVisible: true
                );

                var exp2 = new Experience(
                    jobTitle: "Coding Instructor",
                    companyName: "Udacity, Al Mentor, iSchool",
                    description: "Delivered technical sessions on backend development, software engineering principles, and problem-solving methodologies while mentoring students on real-world programming, Clean Code, and industry development practices.",
                    startDate: new DateTime(2024, 8, 1, 0, 0, 0, DateTimeKind.Utc),
                    endDate: new DateTime(2025, 3, 1, 0, 0, 0, DateTimeKind.Utc),
                    isCurrent: false,
                    displayOrder: 2,
                    isVisible: true
                );

                await _dbContext.Experiences.AddRangeAsync(exp1, exp2);
                _logger.LogInformation("Seeded real Experiences from CV into database.");
            }

            // 4. Seed Projects from real CV if empty
            if (!await _dbContext.Projects.AnyAsync())
            {
                var proj1 = new Project(
                    title: "Social Development Bank",
                    description: "Engineered financial analysis services for transaction aggregation, spending categorization, and account analytics, integrating multiple external banking APIs.",
                    image: "assets/images/project_preview_1.png",
                    liveUrl: "https://github.com/Abdallah72296",
                    githubUrl: "https://github.com/Abdallah72296",
                    displayOrder: 1,
                    isFeatured: true,
                    isVisible: true
                );
                proj1.AddTechnology("ASP.NET Core Web API");
                proj1.AddTechnology("CQRS");
                proj1.AddTechnology("RabbitMQ");
                proj1.AddTechnology("Firebase");

                var proj2 = new Project(
                    title: "MaintenanceVisits",
                    description: "Built and delivered an end-to-end maintenance management system for Gulf Emergency Safety Tools and Materials, streamlining inspections, technical reporting, and spare-part quotation workflows. Designed and implemented RESTful APIs and business workflows using ASP.NET Core, EF Core, SQL Server, Clean Architecture, and CQRS.",
                    image: "assets/images/project_preview_2.png",
                    liveUrl: "https://github.com/Abdallah72296",
                    githubUrl: "https://github.com/Abdallah72296",
                    displayOrder: 2,
                    isFeatured: true,
                    isVisible: true
                );
                proj2.AddTechnology("ASP.NET Core Web API");
                proj2.AddTechnology("Clean Architecture");
                proj2.AddTechnology("CQRS");
                proj2.AddTechnology("EF Core");
                proj2.AddTechnology("SQL Server");

                var proj3 = new Project(
                    title: "Project Management System",
                    description: "Engineered backend services for end-to-end project and task lifecycle management, including authorization, notification workflows, and Google Calendar integration.",
                    image: "assets/images/project_preview_3.png",
                    liveUrl: "https://github.com/Abdallah72296",
                    githubUrl: "https://github.com/Abdallah72296",
                    displayOrder: 3,
                    isFeatured: true,
                    isVisible: true
                );
                proj3.AddTechnology("ASP.NET Core Web API");
                proj3.AddTechnology("JWT");
                proj3.AddTechnology("Google Calendar API");
                proj3.AddTechnology("CQRS");

                var proj4 = new Project(
                    title: "Bustan Mall",
                    description: "Designed and built a full-stack marketplace platform supporting store management, product publishing, and online ordering workflows. Graduation Project graded A+.",
                    image: "assets/images/project_preview_1.png",
                    liveUrl: "https://github.com/Abdallah72296",
                    githubUrl: "https://github.com/Abdallah72296",
                    displayOrder: 4,
                    isFeatured: true,
                    isVisible: true
                );
                proj4.AddTechnology("ASP.NET Core Web API");
                proj4.AddTechnology("Flutter");
                proj4.AddTechnology("React");
                proj4.AddTechnology("SQL Server");

                await _dbContext.Projects.AddRangeAsync(proj1, proj2, proj3, proj4);
                _logger.LogInformation("Seeded real Projects from CV into database.");
            }

            // 5. Seed Education from real CV if empty
            if (!await _dbContext.Educations.AnyAsync())
            {
                var edu = new Education(
                    institutionName: "Sinai University",
                    degree: "Bachelor's Degree in Computer Science & Information Technology",
                    startDate: new DateTime(2020, 10, 1, 0, 0, 0, DateTimeKind.Utc),
                    endDate: new DateTime(2024, 7, 1, 0, 0, 0, DateTimeKind.Utc),
                    isCurrent: false,
                    fieldOfStudy: "Computer Science & Information Technology",
                    description: "Graduation Project graded A+ (Bustan Mall - Enterprise multi-vendor marketplace platform). Comprehensive study of Algorithms, Data Structures, Software Engineering, Database Systems, and Distributed Computing.",
                    displayOrder: 1,
                    isVisible: true
                );

                await _dbContext.Educations.AddAsync(edu);
                _logger.LogInformation("Seeded real Education from CV into database.");
            }

            // 6. Seed Skills & Categories from real CV if empty
            if (!await _dbContext.SkillCategories.AnyAsync())
            {
                var cat1 = new SkillCategory("Languages & Frameworks", 1);
                var cat2 = new SkillCategory("Databases & Messaging", 2);
                var cat3 = new SkillCategory("Architecture & Concepts", 3);
                var cat4 = new SkillCategory("Security & Integration", 4);
                var cat5 = new SkillCategory("Tools & DevOps", 5);

                await _dbContext.SkillCategories.AddRangeAsync(cat1, cat2, cat3, cat4, cat5);
                await _dbContext.SaveChangesAsync();

                // Add skills under categories
                var skills = new List<Skill>
                {
                    new Skill("C#", cat1.Id, 1, "bi-filetype-cs"),
                    new Skill("ASP.NET Core Web API", cat1.Id, 2, "bi-gear-wide-connected"),
                    new Skill("EF Core", cat1.Id, 3, "bi-database-fill-gear"),
                    new Skill("LINQ", cat1.Id, 4, "bi-filter-square"),
                    new Skill("TypeScript", cat1.Id, 5, "bi-filetype-tsx"),
                    new Skill("JavaScript", cat1.Id, 6, "bi-filetype-js"),
                    new Skill("Python", cat1.Id, 7, "bi-filetype-py"),
                    new Skill("C++", cat1.Id, 8, "bi-code-square"),

                    new Skill("SQL Server", cat2.Id, 1, "bi-database"),
                    new Skill("PostgreSQL", cat2.Id, 2, "bi-database-check"),
                    new Skill("RabbitMQ", cat2.Id, 3, "bi-arrow-left-right"),
                    new Skill("Redis", cat2.Id, 4, "bi-lightning-charge-fill"),
                    new Skill("Firebase", cat2.Id, 5, "bi-fire"),

                    new Skill("Clean Architecture", cat3.Id, 1, "bi-diagram-3-fill"),
                    new Skill("CQRS & MediatR", cat3.Id, 2, "bi-arrow-repeat"),
                    new Skill("SOLID Principles", cat3.Id, 3, "bi-layers-half"),
                    new Skill("Repository & Unit of Work", cat3.Id, 4, "bi-collection"),
                    new Skill("System Design", cat3.Id, 5, "bi-grid-1x2-fill"),

                    new Skill("JWT & Identity", cat4.Id, 1, "bi-shield-lock-fill"),
                    new Skill("RESTful APIs", cat4.Id, 2, "bi-hdd-network"),
                    new Skill("FluentValidation", cat4.Id, 3, "bi-check2-circle"),
                    new Skill("Google Calendar API", cat4.Id, 4, "bi-calendar-check"),

                    new Skill("Git & GitHub", cat5.Id, 1, "bi-git"),
                    new Skill("Docker", cat5.Id, 2, "bi-box-seam"),
                    new Skill("Automated Testing", cat5.Id, 3, "bi-check-all")
                };

                await _dbContext.Skills.AddRangeAsync(skills);
                _logger.LogInformation("Seeded real Skills from CV into database.");
            }

            await _dbContext.SaveChangesAsync();
        });
    }
}
