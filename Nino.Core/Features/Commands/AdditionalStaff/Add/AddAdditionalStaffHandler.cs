// SPDX-License-Identifier: MPL-2.0

using Nino.Core.Services;
using Nino.Domain.Entities;
using Nino.Domain.Enums;
using Task = Nino.Domain.Entities.Task;

namespace Nino.Core.Features.Commands.AdditionalStaff.Add;

public sealed class AddAdditionalStaffHandler(
    NinoDbContext db,
    IUserVerificationService verificationService,
    ILogger<AddAdditionalStaffHandler> logger
)
{
    private const decimal DefaultWeight = 1000000m;

    public async Task<Result<GenericResponse>> HandleAsync(AddAdditionalStaffCommand input)
    {
        var (projectId, episodeId, requestedBy) = input;

        if (
            !await verificationService.VerifyProjectPermissionsAsync(
                projectId,
                requestedBy,
                PermissionsLevel.Administrator
            )
        )
            return Result<GenericResponse>.Fail(ResultStatus.Unauthorized);

        var episode = await db.Episodes.SingleAsync(e => e.Id == episodeId);

        if (episode.Tasks.Any(t => t.Abbreviation == input.Abbreviation))
            return Result<GenericResponse>.Fail(ResultStatus.Conflict);

        var newStaff = new Staff
        {
            UserId = input.MemberId,
            Role = new Role
            {
                Abbreviation = input.Abbreviation,
                Name = input.FullName,
                Weight = DefaultWeight,
            },
            IsPseudo = input.IsPseudo,
            EpisodeId = episodeId,
        };
        episode.AdditionalStaff.Add(newStaff);

        var newTask = new Task
        {
            EpisodeId = episodeId,
            Abbreviation = input.Abbreviation,
            IsDone = false,
        };
        episode.Tasks.Add(newTask);

        logger.LogInformation("Added new Additional Staff {Staff} to {Episode}", newStaff, episode);

        await db.SaveChangesAsync();

        var response = await db
            .Projects.Where(p => p.Id == projectId)
            .Select(p => new GenericResponse(p.Title, p.Type, p.PosterUrl))
            .SingleAsync();

        return Result<GenericResponse>.Success(response);
    }
}
