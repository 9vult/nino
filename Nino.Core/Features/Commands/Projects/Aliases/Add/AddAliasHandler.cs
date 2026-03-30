// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nino.Core.Services;
using Nino.Domain.Entities;
using Nino.Domain.Enums;
using static Nino.Core.Features.Result;

namespace Nino.Core.Features.Commands.Projects.Aliases.Add;

public sealed class AddAliasHandler(
    NinoDbContext db,
    IUserVerificationService verificationService,
    ILogger<AddAliasHandler> logger
) : ICommandHandler<AddAliasCommand, Result>
{
    /// <inheritdoc />
    public async Task<Result> HandleAsync(AddAliasCommand command)
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

        if (project.Aliases.Any(a => a.Value == command.Alias) || project.Nickname == command.Alias)
            return Fail(ResultStatus.BadRequest);

        project.Aliases.Add(new ProjectAlias { Value = command.Alias });
        await db.SaveChangesAsync();
        return Success();
    }
}
