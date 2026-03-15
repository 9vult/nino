// SPDX-License-Identifier: MPL-2.0

using NaturalSort.Extension;
using Nino.Core.Services;
using Nino.Domain.Enums;

namespace Nino.Core.Features.Commands.Episodes.Remove;

public sealed class RemoveEpisodeHandler(
    NinoDbContext db,
    IUserVerificationService verificationService,
    ILogger<RemoveEpisodeHandler> logger
)
{
    public async Task<Result<RemoveEpisodeResponse>> HandleAsync(RemoveEpisodeCommand input)
    {
        if (
            !await verificationService.VerifyProjectPermissionsAsync(
                input.ProjectId,
                input.RequestedBy,
                PermissionsLevel.Administrator
            )
        )
            return Result<RemoveEpisodeResponse>.Fail(ResultStatus.Unauthorized);

        var project = await db
            .Projects.Include(p => p.Episodes)
            .SingleAsync(p => p.Id == input.ProjectId);

        var episodes = project
            .Episodes.OrderBy(e => e.Number, StringComparison.OrdinalIgnoreCase.WithNaturalSort())
            .ToList();

        var firstIdx = episodes.FindIndex(e => e.Id == input.FirstEpisodeId);
        var lastIdx = episodes.FindIndex(e => e.Id == input.LastEpisodeId) + 1;

        var deletable = episodes[firstIdx..lastIdx].Select(e => e.Id).ToList();

        logger.LogInformation("Removing {Count} episodes from {Project}", deletable.Count, project);

        await db.Episodes.Where(e => deletable.Contains(e.Id)).ExecuteDeleteAsync();

        return Result<RemoveEpisodeResponse>.Success(
            new RemoveEpisodeResponse(
                project.Title,
                project.Type,
                project.PosterUrl,
                RemovedEpisodeCount: deletable.Count
            )
        );
    }
}
