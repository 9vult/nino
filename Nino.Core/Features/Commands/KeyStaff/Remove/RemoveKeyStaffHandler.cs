// SPDX-License-Identifier: MPL-2.0

using Nino.Core.Services;
using Nino.Domain.Enums;
using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Commands.KeyStaff.Remove;

public sealed class RemoveKeyStaffHandler(
    NinoDbContext db,
    IUserVerificationService verificationService,
    ILogger<RemoveKeyStaffHandler> logger
)
{
    public async Task<Result<RemoveKeyStaffResponse>> HandleAsync(RemoveKeyStaffCommand input)
    {
        if (
            !await verificationService.VerifyProjectPermissionsAsync(
                input.ProjectId,
                input.RequestedBy,
                PermissionsLevel.Administrator
            )
        )
            return Result<RemoveKeyStaffResponse>.Fail(ResultStatus.Unauthorized);

        var project = await db
            .Projects.Include(p => p.Episodes)
            .Where(p => p.Id == input.ProjectId)
            .SingleAsync();

        var staff = project.KeyStaff.Single(s => s.Id == input.StaffId);

        logger.LogInformation("Removing Key Staff {Staff} from {Project}", staff, project);

        project.KeyStaff.Remove(staff);

        List<(EpisodeId, string)> completedEpisodes = [];
        foreach (var episode in project.Episodes)
        {
            var task = episode.Tasks.Single(t => t.Abbreviation == staff.Role.Abbreviation);
            episode.Tasks.Remove(task);
            episode.IsDone = episode.Tasks.All(t => t.IsDone);

            if (episode.IsDone)
                completedEpisodes.Add((episode.Id, episode.Number));
        }

        await db.SaveChangesAsync();

        var response = await db
            .Projects.Where(p => p.Id == input.ProjectId)
            .Select(p => new RemoveKeyStaffResponse(
                p.Title,
                p.Type,
                p.PosterUrl,
                CompletedEpisodes: completedEpisodes
            ))
            .SingleAsync();

        return Result<RemoveKeyStaffResponse>.Success(response);
    }
}
