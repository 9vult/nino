// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NaturalSort.Extension;
using Nino.Core.Services;
using Nino.Domain.Enums;
using Nino.Domain.ValueObjects;
using static Nino.Core.Features.Result<Nino.Core.Features.Commands.Tasks.Remove.RemoveTaskResponse>;

namespace Nino.Core.Features.Commands.Tasks.Remove;

public sealed class RemoveTaskHandler(
    NinoDbContext db,
    IUserVerificationService verificationService,
    ILogger<RemoveTaskHandler> logger
) : ICommandHandler<RemoveTaskCommand, Result<RemoveTaskResponse>>
{
    /// <inheritdoc />
    public async Task<Result<RemoveTaskResponse>> HandleAsync(RemoveTaskCommand command)
    {
        var verification = await verificationService.VerifyProjectPermissionsAsync(
            command.ProjectId,
            command.RequestedBy,
            PermissionsLevel.Administrator
        );
        if (!verification.IsSuccess)
            return Fail(verification.Status);

        var episodes = (
            await db.Episodes.Where(e => e.ProjectId == command.ProjectId).ToListAsync()
        )
            .OrderBy(e => e.Number.Value, StringComparison.OrdinalIgnoreCase.WithNaturalSort())
            .ToList();

        var firstIdx = episodes.FindIndex(e => e.Id == command.FirstEpisodeId);
        var lastIdx = episodes.FindIndex(e => e.Id == command.LastEpisodeId) + 1;

        if (firstIdx < 0)
            return Fail(ResultStatus.EpisodeNotFound, "first");
        if (lastIdx < 1)
            return Fail(ResultStatus.EpisodeNotFound, "last");

        episodes = episodes[firstIdx..lastIdx].ToList();

        if (episodes.SelectMany(e => e.Tasks).All(t => t.Abbreviation != command.Abbreviation))
            return Fail(ResultStatus.TaskNotFound);

        List<(EpisodeId, Number)> completedEpisodes = [];

        // Remove from episodes
        foreach (var episode in episodes)
        {
            var task = episode.Tasks.FirstOrDefault(t => t.Abbreviation == command.Abbreviation);
            if (task is null)
                continue;

            episode.Tasks.Remove(task);
            episode.UpdatedAt = DateTime.UtcNow;
            episode.IsDone = episode.Tasks.All(t => t.IsDone);

            if (episode.IsDone)
                completedEpisodes.Add((episode.Id, episode.Number));
        }

        logger.LogInformation(
            "Removing Task {Task} from project {ProjectId} episodes {FirstEpisode}-{LastEpisode}",
            command.Abbreviation,
            command.ProjectId,
            episodes.First().Number.Value,
            episodes.Last().Number.Value
        );

        await db.SaveChangesAsync();
        return Success(new RemoveTaskResponse(completedEpisodes));
    }
}
