// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nino.Core.Services;
using Nino.Domain.Enums;
using Nino.Domain.ValueObjects;
using static Nino.Core.Features.Result;

namespace Nino.Core.Features.Commands.Projects.Conga.AddGroupMember;

public sealed class AddCongaGroupMemberHandler(
    NinoDbContext db,
    IUserVerificationService verificationService,
    ILogger<AddCongaGroupMemberHandler> logger
) : ICommandHandler<AddCongaGroupMemberCommand, Result>
{
    /// <inheritdoc />
    public async Task<Result> HandleAsync(AddCongaGroupMemberCommand command)
    {
        var verification = await verificationService.VerifyProjectPermissionsAsync(
            command.ProjectId,
            command.RequestedBy,
            PermissionsLevel.Administrator
        );
        if (!verification.IsSuccess)
            return Fail(verification.Status);

        var project = await db
            .Projects.Include(p => p.Episodes)
            .FirstOrDefaultAsync(p => p.Id == command.ProjectId);
        if (project is null)
            return Fail(ResultStatus.ProjectNotFound);

        // Project channel required
        if (project.ProjectChannelId.Value == ChannelId.Unset)
            return Fail(ResultStatus.MissingProjectChannel);

        logger.LogInformation(
            "Adding node {NodeName} to Conga Group {GroupName} in project {ProjectId}'s Conga graph",
            command.NodeName,
            command.GroupName,
            project.Id
        );

        var result = project.CongaParticipants.AddGroupMember(command.GroupName, command.NodeName);
        if (result is CongaModificationResult.Success)
        {
            db.Entry(project).Property(p => p.CongaParticipants).IsModified = true;
            await db.SaveChangesAsync();
            return Success();
        }

        return result switch
        {
            CongaModificationResult.NoGroup => Fail(ResultStatus.BadRequest, "noGroup"),
            CongaModificationResult.Duplicate => Fail(ResultStatus.CongaConflict, "graph"),
            CongaModificationResult.DuplicateMember => Fail(ResultStatus.CongaConflict, "member"),
            _ => Fail(ResultStatus.Error),
        };
    }
}
