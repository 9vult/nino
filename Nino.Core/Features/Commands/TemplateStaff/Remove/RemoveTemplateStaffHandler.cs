// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nino.Core.Services;
using Nino.Domain.Enums;
using Nino.Domain.ValueObjects;
using static Nino.Core.Features.Result<Nino.Core.Features.Commands.TemplateStaff.Remove.RemoveTemplateStaffResponse>;

namespace Nino.Core.Features.Commands.TemplateStaff.Remove;

public sealed class RemoveTemplateStaffHandler(
    NinoDbContext db,
    IUserVerificationService verificationService,
    ILogger<RemoveTemplateStaffHandler> logger
) : ICommandHandler<RemoveTemplateStaffCommand, Result<RemoveTemplateStaffResponse>>
{
    /// <inheritdoc />
    public async Task<Result<RemoveTemplateStaffResponse>> HandleAsync(
        RemoveTemplateStaffCommand command
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
            .SingleOrDefaultAsync(p => p.Id == command.ProjectId);
        if (project is null)
            return Fail(ResultStatus.ProjectNotFound);

        var staff = project.TemplateStaff.FirstOrDefault(t => t.Id == command.TemplateStaffId);
        if (staff is null)
            return Fail(ResultStatus.TemplateStaffNotFound);

        project.TemplateStaff.Remove(staff);

        var episodes = command.Applicator switch
        {
            TemplateStaffApplicator.AllEpisodes => project.Episodes,
            TemplateStaffApplicator.IncompleteEpisodes => project
                .Episodes.Where(e => !e.IsDone)
                .ToList(),
            _ => [],
        };

        List<(EpisodeId, Number)> completedEpisodes = [];

        // Remove from episodes
        foreach (var episode in episodes)
        {
            var task = episode.Tasks.FirstOrDefault(t => t.Abbreviation == staff.Abbreviation);
            if (task is null)
                continue;

            episode.Tasks.Remove(task);
            episode.UpdatedAt = DateTime.UtcNow;
            episode.IsDone = episode.Tasks.All(t => t.IsDone);

            if (episode.IsDone)
                completedEpisodes.Add((episode.Id, episode.Number));
        }

        logger.LogInformation(
            "Removing Template Staff {Abbreviation} from project {ProjectId} and applying to {EpisodeCount} episodes",
            staff.Abbreviation,
            project.Id,
            episodes.Count
        );

        await db.SaveChangesAsync();
        return Success(new RemoveTemplateStaffResponse(completedEpisodes));
    }
}
