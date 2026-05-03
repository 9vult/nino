// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nino.Core.Services;
using Nino.Domain.Enums;
using static Nino.Core.Features.Result;

namespace Nino.Core.Features.Commands.Projects.Aliases.Remove;

public sealed class RemoveAliasHandler(
    NinoDbContext db,
    IUserVerificationService verificationService,
    ILogger<RemoveAliasHandler> logger
) : ICommandHandler<RemoveAliasCommand, Result>
{
    /// <inheritdoc />
    public async Task<Result> HandleAsync(RemoveAliasCommand command)
    {
        var verification = await verificationService.VerifyProjectPermissionsAsync(
            command.ProjectId,
            command.RequestedBy,
            PermissionsLevel.Administrator
        );
        if (!verification.IsSuccess)
            return Fail(verification.Status);

        var project = await db.Projects.Where(p => p.Id == command.ProjectId).FirstOrDefaultAsync();
        if (project is null)
            return Fail(ResultStatus.ProjectNotFound);

        var alias = project.Aliases.FirstOrDefault(a => a.Value == command.Alias);
        if (alias is null)
            return Fail(ResultStatus.BadRequest);

        logger.LogInformation(
            "Removed alias {Alias} from project {ProjectId}",
            command.Alias,
            command.ProjectId
        );

        project.Aliases.Remove(alias);
        await db.SaveChangesAsync();
        return Success();
    }
}
