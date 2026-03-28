// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NaturalSort.Extension;
using Nino.Core.Services;
using Nino.Domain.Enums;
using static Nino.Core.Features.Result<Nino.Core.Features.Commands.Episodes.Remove.RemoveEpisodeResponse>;

namespace Nino.Core.Features.Commands.Episodes.Remove;

public sealed class RemoveEpisodeHandler(
    NinoDbContext db,
    IUserVerificationService verificationService,
    ILogger<RemoveEpisodeHandler> logger
) : ICommandHandler<RemoveEpisodeCommand, Result<RemoveEpisodeResponse>>
{
    /// <inheritdoc />
    public async Task<Result<RemoveEpisodeResponse>> HandleAsync(RemoveEpisodeCommand command)
    {
        var verification = await verificationService.VerifyProjectPermissionsAsync(
            command.ProjectId,
            command.RequestedBy,
            PermissionsLevel.Administrator
        );
        if (!verification.IsSuccess)
            return Fail(verification.Status);

        var episodes = await db.Episodes.Where(e => e.ProjectId == command.ProjectId).ToListAsync();
        episodes = episodes
            .OrderBy(e => e.Number.Value, StringComparison.OrdinalIgnoreCase.WithNaturalSort())
            .ToList();

        var firstIdx = episodes.FindIndex(e => e.Id == command.FirstEpisodeId);
        var lastIdx = episodes.FindIndex(e => e.Id == command.LastEpisodeId) + 1;

        if (firstIdx < 0)
            return Fail(ResultStatus.EpisodeNotFound, "first");
        if (lastIdx < 1)
            return Fail(ResultStatus.EpisodeNotFound, "last");

        var deletable = episodes[firstIdx..lastIdx].Select(e => e.Id).ToList();

        logger.LogInformation(
            "Removing {Count} episodes from project {ProjectId}",
            deletable.Count,
            command.ProjectId
        );

        await db.Episodes.Where(e => deletable.Contains(e.Id)).ExecuteDeleteAsync();
        return Success(new RemoveEpisodeResponse(deletable.Count));
    }
}
