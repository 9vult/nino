// SPDX-License-Identifier: MPL-2.0

using Nino.Core.Entities;
using Nino.Core.Enums;
using Nino.Core.Services;

namespace Nino.Core.Actions.KeyStaff.Add;

public sealed class KeyStaffAddHandler(
    DataContext db,
    IUserVerificationService verificationService,
    ILogger<KeyStaffAddHandler> logger
)
{
    public async Task<Result> HandleAsync(KeyStaffAddAction action)
    {
        var (projectId, userId, abbreviation, fullName, isPseudo, requestedBy) = action;

        if (
            !await verificationService.VerifyProjectPermissionsAsync(
                projectId,
                requestedBy,
                PermissionsLevel.Administrator
            )
        )
            return new Result(ResultStatus.Unauthorized);

        var project = await db
            .Projects.Include(p => p.Episodes)
            .SingleOrDefaultAsync(p => p.Id == projectId);

        if (project is null)
            return new Result(ResultStatus.NotFound);

        var taskAlreadyExists =
            project.KeyStaff.Any(s => s.Role.Abbreviation == abbreviation)
            || project
                .Episodes.SelectMany(e => e.AdditionalStaff)
                .Any(s => s.Role.Abbreviation == abbreviation);
        var maxWeight = project.KeyStaff.Count > 0 ? project.KeyStaff.Max(s => s.Role.Weight) : 0;

        if (taskAlreadyExists)
            return new Result(ResultStatus.Conflict);

        var newStaff = new Staff
        {
            UserId = userId,
            Role = new Role
            {
                Abbreviation = abbreviation,
                Name = fullName,
                Weight = maxWeight + 1,
            },
            IsPseudo = isPseudo,
        };

        logger.LogInformation("Adding new Key Staff {Staff} to {Project}", newStaff, project);

        project.KeyStaff.Add(newStaff);
        await db.SaveChangesAsync();
        return new Result(ResultStatus.Success);
    }
}
