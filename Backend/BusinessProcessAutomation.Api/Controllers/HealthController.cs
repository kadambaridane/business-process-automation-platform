using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BusinessProcessAutomation.Api.Data;

namespace BusinessProcessAutomation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public HealthController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public IActionResult GetStatus()
    {
        return Ok(new { status = "Healthy", timestamp = DateTime.UtcNow });
    }

    [HttpGet("db")]
    public async Task<IActionResult> GetDatabaseStatus()
    {
        var canConnect = await _dbContext.Database.CanConnectAsync();
        return Ok(new { databaseConnected = canConnect, timestamp = DateTime.UtcNow });
    }
}
