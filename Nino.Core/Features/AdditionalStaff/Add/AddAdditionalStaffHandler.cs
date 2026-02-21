// SPDX-License-Identifier: MPL-2.0

using Nino.Core.Entities;
using Nino.Core.Enums;
using Nino.Core.Services;
using Task = Nino.Core.Entities.Task;

namespace Nino.Core.Features.AdditionalStaff.Add;

public sealed class AddAdditionalStaffHandler(
    DataContext db,
    IUserVerificationService verificationService,
    ILogger<AddAdditionalStaffHandler> logger
)
{
    private const decimal DefaultWeight = 1000000m;

    public async Task<Result> HandleAsync(AddAdditionalStaffCommand action)
    {
        var (projectId, userId, episodeNumber, abbreviation, fullName, isPseudo, requestedBy) =
            action;

        if (
            !await verificationService.VerifyProjectPermissionsAsync(
                projectId,
                requestedBy,
                PermissionsLevel.Administrator
            )
        )
            return new Result(ResultStatus.Unauthorized);

        var episode = await db
            .Episodes.Where(p => p.ProjectId == projectId)
            .SingleOrDefaultAsync(p => p.Number == episodeNumber);
        if (episode is null)
            return new Result(ResultStatus.NotFound);

        var taskAlreadyExists = episode.Tasks.Any(t => t.Abbreviation == abbreviation);
        if (taskAlreadyExists)
            return new Result(ResultStatus.Conflict);

        var newStaff = new Staff
        {
            UserId = userId,
            Role = new Role
            {
                Abbreviation = abbreviation,
                Name = fullName,
                Weight = DefaultWeight,
            },
            IsPseudo = isPseudo,
        };

        logger.LogInformation("Added new Additional Staff {Staff} to {Episode}", newStaff, episode);

        episode.AdditionalStaff.Add(newStaff);
        episode.Tasks.Add(new Task { Abbreviation = abbreviation, IsDone = false });
        episode.IsDone = false;

        await db.SaveChangesAsync();
        return new Result(ResultStatus.Success);
    }
}
