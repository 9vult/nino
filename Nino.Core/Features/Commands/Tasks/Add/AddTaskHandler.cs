// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NaturalSort.Extension;
using Nino.Core.Services;
using Nino.Domain.Enums;
using static Nino.Core.Features.Result;
using Task = Nino.Domain.Entities.Task;

namespace Nino.Core.Features.Commands.Tasks.Add;

public sealed class AddTaskHandler(
    NinoDbContext db,
    IUserVerificationService verificationService,
    ILogger<AddTaskHandler> logger
) : ICommandHandler<AddTaskCommand, Result>
{
    /// <inheritdoc />
    public async Task<Result> HandleAsync(AddTaskCommand command)
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

        // Check for conflicts
        if (episodes.Any(e => e.Tasks.Any(t => t.Abbreviation == command.Abbreviation)))
            return Fail(ResultStatus.TaskConflict);

        // Add to episodes
        foreach (var episode in episodes)
        {
            episode.Tasks.Add(
                new Task
                {
                    ProjectId = command.ProjectId,
                    EpisodeId = episode.Id,
                    AssigneeId = command.AssigneeId,
                    Abbreviation = command.Abbreviation,
                    Name = command.Name,
                    Weight = episode.Tasks.Select(s => s.Weight).DefaultIfEmpty(0).Max() + 1,
                    IsPseudo = command.IsPseudo,
                    IsDone = false,
                }
            );
            episode.IsDone = false;
            episode.UpdatedAt = DateTime.UtcNow;
        }

        logger.LogInformation(
            "Adding Task {Abbreviation} to project {ProjectId} and applying to {EpisodeCount} episodes",
            command.Abbreviation,
            command.ProjectId,
            episodes.Count
        );

        await db.SaveChangesAsync();
        return Success();
    }
}
