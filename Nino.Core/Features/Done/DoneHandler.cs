// SPDX-License-Identifier: MPL-2.0

using Nino.Core.Entities;
using Nino.Core.Enums;
using Nino.Core.Events;
using Nino.Core.Services;

namespace Nino.Core.Features.Done;

public partial class DoneHandler(
    DataContext db,
    IUserVerificationService verificationService,
    IEventBus eventBus,
    ILogger<DoneHandler> logger
)
{
    public async Task<Result<bool>> HandleAsync(DoneCommand action)
    {
        var (projectId, episodeNumber, abbreviation, requestedBy) = action;
        var episode = await db.Episodes.SingleOrDefaultAsync(e =>
            e.Id == projectId && e.Number == episodeNumber
        );

        if (episode is null)
            return new Result<bool>(ResultStatus.NotFound);

        var episodeId = episode.Id;

        if (
            !await verificationService.VerifyTaskPermissionsAsync(
                projectId,
                episodeId,
                requestedBy,
                abbreviation
            )
        )
            return new Result<bool>(ResultStatus.Unauthorized);

        var task = episode.Tasks.SingleOrDefault(t => t.Abbreviation == abbreviation);
        if (task is null)
            return new Result<bool>(ResultStatus.NotFound);

        if (task.IsDone)
            return new Result<bool>(ResultStatus.BadRequest);

        task.IsDone = true;
        task.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync();

        await eventBus.PublishAsync(
            new TaskCompletedEvent(projectId, episodeId, abbreviation, false, DateTimeOffset.UtcNow)
        );

        return new Result<bool>(ResultStatus.Success, episode.Tasks.All(t => t.IsDone));
    }
}
