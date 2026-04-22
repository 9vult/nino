// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nino.Core.Services;
using Nino.Domain.Enums;
using static Nino.Core.Features.Result;
using Task = Nino.Domain.Entities.Task;

namespace Nino.Core.Features.Commands.TemplateStaff.Add;

public sealed class AddTemplateStaffHandler(
    NinoDbContext db,
    IUserVerificationService verificationService,
    ILogger<AddTemplateStaffHandler> logger
) : ICommandHandler<AddTemplateStaffCommand, Result>
{
    /// <inheritdoc />
    public async Task<Result> HandleAsync(AddTemplateStaffCommand command)
    {
        var verification = await verificationService.VerifyProjectPermissionsAsync(
            command.ProjectId,
            command.RequestedBy,
            PermissionsLevel.Administrator
        );
        if (!verification.IsSuccess)
            return Fail(verification.Status);

        var project = await db
            .Projects.Include(p => p.Episodes)
            .SingleOrDefaultAsync(p => p.Id == command.ProjectId);
        if (project is null)
            return Fail(ResultStatus.ProjectNotFound);

        // Check for conflicts
        if (project.TemplateStaff.Any(s => s.Abbreviation == command.Abbreviation))
            return Fail(ResultStatus.TaskConflict);

        switch (command.Applicator)
        {
            case TemplateStaffApplicator.FutureEpisodes:
                break;
            case TemplateStaffApplicator.IncompleteEpisodes:
                if (
                    project
                        .Episodes.Where(e => !e.IsDone)
                        .SelectMany(e => e.Tasks)
                        .Any(t => t.Abbreviation == command.Abbreviation)
                )
                    return Fail(ResultStatus.TaskConflict);
                break;
            case TemplateStaffApplicator.AllEpisodes:
                if (
                    project
                        .Episodes.SelectMany(e => e.Tasks)
                        .Any(t => t.Abbreviation == command.Abbreviation)
                )
                    return Fail(ResultStatus.TaskConflict);
                break;
        }

        var maxWeight = project.TemplateStaff.Select(s => s.Weight).DefaultIfEmpty(0).Max();

        // Add to the template staff
        project.TemplateStaff.Add(
            new Domain.Entities.TemplateStaff
            {
                ProjectId = project.Id,
                AssigneeId = command.AssigneeId,
                Abbreviation = command.Abbreviation,
                Name = command.Name,
                Weight = maxWeight + 1,
                IsPseudo = command.IsPseudo,
            }
        );

        var episodes = command.Applicator switch
        {
            TemplateStaffApplicator.AllEpisodes => project.Episodes,
            TemplateStaffApplicator.IncompleteEpisodes => project
                .Episodes.Where(e => !e.IsDone)
                .ToList(),
            _ => [],
        };

        // Add to episodes
        foreach (var episode in episodes)
        {
            episode.Tasks.Add(
                new Task
                {
                    ProjectId = project.Id,
                    EpisodeId = episode.Id,
                    AssigneeId = command.AssigneeId,
                    Abbreviation = command.Abbreviation,
                    Name = command.Name,
                    Weight = maxWeight,
                    IsPseudo = command.IsPseudo,
                    IsDone = false,
                }
            );
            episode.IsDone = false;
            episode.UpdatedAt = DateTime.UtcNow;
        }

        logger.LogInformation(
            "Adding Template Staff {Abbreviation} to project {ProjectId} and applying to {EpisodeCount} episodes",
            command.Abbreviation,
            project.Id,
            episodes.Count
        );

        await db.SaveChangesAsync();
        return Success();
    }
}
