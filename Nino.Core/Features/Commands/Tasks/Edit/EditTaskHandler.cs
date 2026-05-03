// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NaturalSort.Extension;
using Nino.Core.Services;
using Nino.Domain.Enums;
using static Nino.Core.Features.Result;

namespace Nino.Core.Features.Commands.Tasks.Edit;

public sealed class EditTaskHandler(
    NinoDbContext db,
    IUserVerificationService verificationService,
    ILogger<EditTaskHandler> logger
) : ICommandHandler<EditTaskCommand, Result>
{
    /// <inheritdoc />
    public async Task<Result> HandleAsync(EditTaskCommand command)
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

        // Check for conflicts, if applicable
        if (command.NewAbbreviation is not null)
        {
            if (
                episodes
                    .SelectMany(e => e.Tasks)
                    .Any(t => t.Abbreviation == command.NewAbbreviation)
            )
                return Fail(ResultStatus.TaskConflict);
        }

        var tasks = episodes
            .SelectMany(e => e.Tasks)
            .Where(t => t.Abbreviation == command.Abbreviation)
            .ToList();

        if (command.AssigneeId is not null)
        {
            logger.LogInformation(
                "Setting Task {Task} for project {ProjectId} episodes {FirstEpisode}-{LastEpisode}'s Assignee to {AssigneeId}.",
                command.Abbreviation,
                command.ProjectId,
                episodes.First().Number.Value,
                episodes.Last().Number.Value,
                command.AssigneeId.Value
            );
            foreach (var task in tasks)
                task.AssigneeId = command.AssigneeId.Value;
        }

        if (command.NewAbbreviation is not null)
        {
            logger.LogInformation(
                "Setting Task {Task} for project {ProjectId} episodes {FirstEpisode}-{LastEpisode}'s Abbreviation to {Abbreviation}.",
                command.Abbreviation,
                command.ProjectId,
                episodes.First().Number.Value,
                episodes.Last().Number.Value,
                command.NewAbbreviation.Value
            );
            foreach (var task in tasks)
                task.Abbreviation = command.NewAbbreviation.Value;
        }

        if (command.Name is not null)
        {
            logger.LogInformation(
                "Setting Task {Task} for project {ProjectId} episodes {FirstEpisode}-{LastEpisode}'s Name to {Name}.",
                command.Abbreviation,
                command.ProjectId,
                episodes.First().Number.Value,
                episodes.Last().Number.Value,
                command.Name
            );
            foreach (var task in tasks)
                task.Name = command.Name;
        }

        if (command.Weight is not null)
        {
            logger.LogInformation(
                "Setting Task {Task} for project {ProjectId} episodes {FirstEpisode}-{LastEpisode}'s Weight to {Weight}.",
                command.Abbreviation,
                command.ProjectId,
                episodes.First().Number.Value,
                episodes.Last().Number.Value,
                command.Weight.Value
            );
            foreach (var task in tasks)
                task.Weight = command.Weight.Value;
        }

        if (command.IsPseudo is not null)
        {
            logger.LogInformation(
                "Setting Task {Task} for project {ProjectId} episodes {FirstEpisode}-{LastEpisode}'s IsPseudo to {IsPseudo}.",
                command.Abbreviation,
                command.ProjectId,
                episodes.First().Number.Value,
                episodes.Last().Number.Value,
                command.IsPseudo.Value
            );
            foreach (var task in tasks)
                task.IsPseudo = command.IsPseudo.Value;
        }

        await db.SaveChangesAsync();
        return Success();
    }
}
