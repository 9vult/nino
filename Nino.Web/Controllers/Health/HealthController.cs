// SPDX-License-Identifier: MPL-2.0

using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Nino.Core;

namespace Nino.Web.Controllers.Health;

[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class HealthController(NinoDbContext db, ILogger<HealthController> logger)
    : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<HealthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetHealthAsync()
    {
        try
        {
            await db.Database.CanConnectAsync();

            return Ok(new HealthResponse("healthy", DateTimeOffset.UtcNow));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Health check failed");

            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                new HealthResponse("unhealthy", DateTimeOffset.UtcNow)
            );
        }
    }
}
