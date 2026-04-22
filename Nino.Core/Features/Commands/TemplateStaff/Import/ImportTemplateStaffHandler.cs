// SPDX-License-Identifier: MPL-2.0

using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nino.Core.Dtos;
using Nino.Core.Features.Commands.Tasks.Add;
using Nino.Core.Services;
using Nino.Domain.Enums;
using Nino.Domain.ValueObjects;
using static Nino.Core.Features.Result<Nino.Core.Features.Commands.TemplateStaff.Import.ImportTemplateStaffResponse>;

namespace Nino.Core.Features.Commands.TemplateStaff.Import;

public sealed class ImportTemplateStaffHandler(
    NinoDbContext db,
    IIdentityService identityService,
    IUserVerificationService verificationService,
    ILogger<AddTaskHandler> logger
) : ICommandHandler<ImportTemplateStaffCommand, Result<ImportTemplateStaffResponse>>
{
    /// <inheritdoc />
    public async Task<Result<ImportTemplateStaffResponse>> HandleAsync(
        ImportTemplateStaffCommand command
    )
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
            .FirstOrDefaultAsync(p => p.Id == command.ProjectId);
        if (project is null)
            return Fail(ResultStatus.ProjectNotFound);

        var added = 0;
        var lines = command.Data.Split(Environment.NewLine);
        foreach (var line in lines)
        {
            var input = JsonSerializer.Deserialize<TemplateStaffImportDto>(line);
            if (input is null)
            {
                logger.LogWarning("Failed to deserialize template staff import \"{Input}\"", line);
                continue;
            }

            UserId assigneeId;
            // Assignee lookup
            if (input.Assignee.Id is not null)
                assigneeId = UserId.From(input.Assignee.Id.Value);
            else if (input.Assignee.DiscordId is not null)
                assigneeId = await identityService.GetOrCreateUserByDiscordIdAsync(
                    input.Assignee.DiscordId.Value
                );
            else
                continue;

            // Check for conflicts
            if (project.TemplateStaff.Any(s => s.Abbreviation == input.Abbreviation))
                return Fail(ResultStatus.TaskConflict);

            switch (input.Applicator)
            {
                case TemplateStaffApplicator.FutureEpisodes:
                    break;
                case TemplateStaffApplicator.IncompleteEpisodes:
                    if (
                        project
                            .Episodes.Where(e => !e.IsDone)
                            .SelectMany(e => e.Tasks)
                            .Any(t => t.Abbreviation == input.Abbreviation)
                    )
                        return Fail(ResultStatus.TaskConflict);
                    break;
                case TemplateStaffApplicator.AllEpisodes:
                    if (
                        project
                            .Episodes.SelectMany(e => e.Tasks)
                            .Any(t => t.Abbreviation == input.Abbreviation)
                    )
                        return Fail(ResultStatus.TaskConflict);
                    break;
            }

            var weight =
                input.Weight
                ?? project.TemplateStaff.Select(s => s.Weight).DefaultIfEmpty(0).Max() + 1;

            // Add to the template staff
            project.TemplateStaff.Add(
                new Domain.Entities.TemplateStaff
                {
                    ProjectId = project.Id,
                    AssigneeId = assigneeId,
                    Abbreviation = input.Abbreviation,
                    Name = input.Name,
                    Weight = weight,
                    IsPseudo = input.IsPseudo,
                }
            );

            var episodes = input.Applicator switch
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
                    new Domain.Entities.Task
                    {
                        ProjectId = command.ProjectId,
                        EpisodeId = episode.Id,
                        AssigneeId = assigneeId,
                        Abbreviation = input.Abbreviation,
                        Name = input.Name,
                        Weight = weight,
                        IsPseudo = input.IsPseudo,
                        IsDone = false,
                    }
                );
                episode.IsDone = false;
                episode.UpdatedAt = DateTime.UtcNow;
            }

            logger.LogInformation(
                "Adding Template Staff {Abbreviation} to project {ProjectId} and applying to {EpisodeCount} episodes",
                input.Abbreviation,
                project.Id,
                episodes.Count
            );
            added++;
        }

        await db.SaveChangesAsync();
        return Success(new ImportTemplateStaffResponse(added));
    }
}
