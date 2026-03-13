// SPDX-License-Identifier: MPL-2.0

using Nino.Core.Services;
using Nino.Domain.Entities;
using Nino.Domain.Enums;
using Task = Nino.Domain.Entities.Task;

namespace Nino.Core.Features.Commands.KeyStaff.Add;

public sealed class AddKeyStaffHandler(
    NinoDbContext db,
    IUserVerificationService verificationService,
    ILogger<AddKeyStaffHandler> logger
)
{
    public async Task<Result<GenericResponse>> HandleAsync(AddKeyStaffCommand input)
    {
        var (projectId, requestedBy, markDone) = input;

        if (
            !await verificationService.VerifyProjectPermissionsAsync(
                projectId,
                requestedBy,
                PermissionsLevel.Administrator
            )
        )
            return Result<GenericResponse>.Fail(ResultStatus.Unauthorized);

        var project = await db
            .Projects.Include(p => p.Episodes)
            .SingleAsync(p => p.Id == projectId);

        if (
            project.Episodes.SelectMany(e => e.Tasks).Any(t => t.Abbreviation == input.Abbreviation)
        )
            return Result<GenericResponse>.Fail(ResultStatus.Conflict);

        var maxWeight = project.KeyStaff.Select(s => s.Role.Weight).DefaultIfEmpty(0).Max();

        var newStaff = new Staff
        {
            UserId = input.MemberId,
            Role = new Role
            {
                Abbreviation = input.Abbreviation,
                Name = input.FullName,
                Weight = maxWeight + 1,
            },
            IsPseudo = input.IsPseudo,
            ProjectId = project.Id,
        };
        project.KeyStaff.Add(newStaff);

        foreach (var episode in project.Episodes)
        {
            episode.Tasks.Add(
                new Task
                {
                    EpisodeId = episode.Id,
                    Abbreviation = input.Abbreviation,
                    IsDone = episode.IsDone && markDone,
                }
            );
        }

        logger.LogInformation("Added new Key Staff {Staff} to {Project}", newStaff, project.Id);

        await db.SaveChangesAsync();
        return Result<GenericResponse>.Success(
            new GenericResponse(project.Title, project.Type, project.PosterUrl)
        );
    }
}
