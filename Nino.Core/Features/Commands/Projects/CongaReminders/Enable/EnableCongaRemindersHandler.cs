// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nino.Core.Services;
using Nino.Domain.Enums;
using Nino.Domain.ValueObjects;
using static Nino.Core.Features.Result;

namespace Nino.Core.Features.Commands.Projects.CongaReminders.Enable;

public sealed class EnableCongaRemindersHandler(
    NinoDbContext db,
    IUserVerificationService verificationService,
    ILogger<EnableCongaRemindersHandler> logger
) : ICommandHandler<EnableCongaRemindersCommand, Result>
{
    /// <inheritdoc />
    public async Task<Result> HandleAsync(EnableCongaRemindersCommand command)
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

        // Project channel required
        if (project.ProjectChannelId.Value == ChannelId.Unset)
            return Fail(ResultStatus.MissingProjectChannel);

        project.CongaRemindersEnabled = true;
        project.CongaReminderPeriod = command.Period;

        logger.LogInformation(
            "Enabling Conga Reminders for project {ProjectId} with period {Period}",
            project.Id,
            command.Period
        );

        await db.SaveChangesAsync();
        return Success();
    }
}
