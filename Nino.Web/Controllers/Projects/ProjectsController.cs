// SPDX-License-Identifier: MPL-2.0

using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nino.Core.Features.Queries.Episodes.GetWorkingEpisode;
using Nino.Domain.ValueObjects;

namespace Nino.Web.Controllers.Projects;

[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class ProjectsController(GetWorkingEpisodeHandler workingEpisodeHandler)
    : ControllerBase
{
    [HttpGet("{projectId}/working-episode")]
    [ProducesResponseType<GetWorkingEpisodeResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetWorkingEpisodeAsync(ProjectId projectId)
    {
        var result = await workingEpisodeHandler.HandleAsync(new GetWorkingEpisodeQuery(projectId));
        return result.IsSuccess
            ? Ok(result.Value)
            : StatusCode(StatusCodes.Status404NotFound, new { result.Status, result.Message });
    }
}
