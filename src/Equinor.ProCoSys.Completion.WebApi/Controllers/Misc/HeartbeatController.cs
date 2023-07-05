using System.ComponentModel.DataAnnotations;
using Equinor.ProCoSys.Common.Telemetry;
using Equinor.ProCoSys.Common.Time;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.Completion.WebApi.Controllers.Misc;

[ApiController]
[Route("Heartbeat")]
public class HeartbeatController : ControllerBase
{
    private readonly ILogger<HeartbeatController> _logger;
    private readonly ITelemetryClient _telemetryClient;

    public HeartbeatController(ILogger<HeartbeatController> logger, ITelemetryClient telemetryClient)
    {
        _logger = logger;
        _telemetryClient = telemetryClient;
    }

    [AllowAnonymous]
    [HttpGet("IsAlive")]
    public IActionResult IsAlive()
    {
        var timestamp = $"{TimeService.UtcNow:yyyy-MM-dd HH:mm:ss} UTC";
        _logger.LogInformation("The application is running at {Timestamp}", timestamp);
        return new JsonResult(new
        {
            IsAlive = true,
            TimeStamp = timestamp
        });
    }

    [HttpPost("LogTrace")]
    public void LogTrace()
        => _telemetryClient.TrackEvent("Heartbeat");

    [HttpPost("LogInfo")]
    public void LogInfo([Required] string info)
        => _logger.LogInformation("Information: {Info}", info);

    [HttpPost("LogWarning")]
    public void LogWarning([Required] string warning)
        => _logger.LogWarning("Warning: {Warning}", warning);

    [HttpPost("LogError")]
    public void LogError([Required] string error)
        => _logger.LogError("Error: {Error}", error);

    [HttpPost("LogException")]
    public void LogException([Required] string error)
        => _logger.LogError(new System.Exception(error), "Exception: {Error}", error);
}
