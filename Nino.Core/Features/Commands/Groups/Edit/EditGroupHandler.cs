// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nino.Core.Services;
using Nino.Domain.Enums;
using static Nino.Core.Features.Result;

namespace Nino.Core.Features.Commands.Groups.Edit;

public sealed class EditGroupHandler(
    NinoDbContext db,
    IUserVerificationService verificationService,
    ILogger<EditGroupHandler> logger
) : ICommandHandler<EditGroupCommand, Result>
{
    /// <inheritdoc />
    public async Task<Result> HandleAsync(EditGroupCommand command)
    {
        var verification = await verificationService.VerifyGroupPermissionsAsync(
            command.GroupId,
            command.RequestedBy,
            PermissionsLevel.Administrator
        );
        if (!command.OverrideVerification && !verification.IsSuccess)
            return Fail(verification.Status);

        var group = await db.Groups.FirstOrDefaultAsync(g => g.Id == command.GroupId);
        if (group is null)
            return Fail(ResultStatus.GroupNotFound);
        var config = group.Configuration;

        if (command.Locale is not null)
        {
            logger.LogTrace(
                "Setting group {GroupId}'s locale to {Locale}",
                group.Id,
                command.Locale.Value
            );
            config.Locale = command.Locale.Value;
        }

        if (command.ProgressResponseType is not null)
        {
            logger.LogTrace(
                "Setting group {GroupId}'s progress response type to {ResponseType}",
                group.Id,
                command.ProgressResponseType.Value
            );
            config.ProgressResponseType = command.ProgressResponseType.Value;
        }

        if (command.ProgressPublishType is not null)
        {
            logger.LogTrace(
                "Setting group {GroupId}'s progress publish type to {PublishType}",
                group.Id,
                command.ProgressPublishType.Value
            );
            config.ProgressPublishType = command.ProgressPublishType.Value;
        }

        if (command.CongaPrefixType is not null)
        {
            logger.LogTrace(
                "Setting group {GroupId}'s conga prefix type to {CongaPrefixType}",
                group.Id,
                command.CongaPrefixType.Value
            );
            config.CongaPrefixType = command.CongaPrefixType.Value;
        }

        if (command.ReleasePrefix is not null)
        {
            logger.LogTrace(
                "Setting group {GroupId}'s release prefix to {ReleasePrefix}",
                group.Id,
                command.ReleasePrefix
            );
            config.ReleasePrefix = command.ReleasePrefix;
        }

        await db.SaveChangesAsync();
        return Success();
    }
}
