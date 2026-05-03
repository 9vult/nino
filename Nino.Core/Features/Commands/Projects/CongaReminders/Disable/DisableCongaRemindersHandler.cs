// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nino.Core.Services;
using Nino.Domain.Enums;
using static Nino.Core.Features.Result;

namespace Nino.Core.Features.Commands.Projects.CongaReminders.Disable;

public sealed class DisableCongaRemindersHandler(
    NinoDbContext db,
    IUserVerificationService verificationService,
    ILogger<DisableCongaRemindersHandler> logger
) : ICommandHandler<DisableCongaRemindersCommand, Result>
{
    /// <inheritdoc />
    public async Task<Result> HandleAsync(DisableCongaRemindersCommand command)
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

        project.CongaRemindersEnabled = false;

        logger.LogInformation("Disabling Conga Reminders for project {ProjectId}", project.Id);

        await db.SaveChangesAsync();
        return Success();
    }
}
