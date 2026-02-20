// SPDX-License-Identifier: MPL-2.0

using Nino.Core.Entities;
using Nino.Core.Enums;
using Nino.Core.Services;
using Nino.Core.Utilities;

namespace Nino.Core.Features.KeyStaff.Remove;

public sealed class RemoveKeyStaffHandler(
    DataContext db,
    IUserVerificationService verificationService,
    ILogger<RemoveKeyStaffHandler> logger
)
{
    public async Task<Result<RemoveKeyStaffResponse>> HandleAsync(RemoveKeyStaffCommand action)
    {
        var (projectId, abbreviation, requestedBy) = action;

        if (
            !await verificationService.VerifyProjectPermissionsAsync(
                projectId,
                requestedBy,
                PermissionsLevel.Administrator
            )
        )
            return new Result<RemoveKeyStaffResponse>(ResultStatus.Unauthorized);

        var project = await db
            .Projects.Include(p => p.Episodes)
            .SingleOrDefaultAsync(p => p.Id == projectId);

        if (project is null)
            return new Result<RemoveKeyStaffResponse>(ResultStatus.NotFound);

        var staff = project.KeyStaff.SingleOrDefault(s => s.Role.Abbreviation == abbreviation);
        if (staff is null)
            return new Result<RemoveKeyStaffResponse>(ResultStatus.NotFound);

        logger.LogInformation("Removing Key Staff {Staff} from {Project}", staff, project);

        project.KeyStaff.Remove(staff);

        List<string> completedEpisodeNumbers = [];
        foreach (var episode in project.Episodes)
        {
            episode.Tasks.RemoveAll(t => t.Abbreviation == abbreviation);
            if (!episode.Tasks.All(t => t.IsDone))
                continue;

            episode.IsDone = true;
            completedEpisodeNumbers.Add(episode.Number);
        }

        await db.SaveChangesAsync();
        return new Result<RemoveKeyStaffResponse>(
            ResultStatus.Success,
            new RemoveKeyStaffResponse(completedEpisodeNumbers)
        );
    }
}
