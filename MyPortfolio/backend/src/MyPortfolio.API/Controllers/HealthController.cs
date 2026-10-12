using Microsoft.AspNetCore.Mvc;
using MyPortfolio.Application.Common.Models;
using MyPortfolio.Infrastructure.Data;

namespace MyPortfolio.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<HealthController> _logger;

    public HealthController(ApplicationDbContext dbContext, ILogger<HealthController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult GetStatus()
    {
        return Ok(ApiResponse<object>.SuccessResult(new
        {
            Status = "Healthy",
            Service = "MyPortfolio Clean Architecture API",
            Timestamp = DateTime.UtcNow,
            Framework = ".NET 10"
        }, "API service is active."));
    }

    [HttpGet("db")]
    public async Task<IActionResult> TestDatabaseConnection(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Testing SQL Server database connectivity...");

        try
        {
            var canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);
            if (canConnect)
            {
                return Ok(ApiResponse<object>.SuccessResult(new
                {
                    Connected = true,
                    Provider = _dbContext.Database.ProviderName,
                    ServerTime = DateTime.UtcNow
                }, "Successfully connected to SQL Server database."));
            }

            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                ApiResponse<object>.FailureResult("Failed to connect to SQL Server database."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database connection test failed.");
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<object>.FailureResult("An internal database error occurred while testing connection. Check server logs."));
        }
    }
}
