// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nino.Core.Services;
using Nino.Domain.Enums;
using static Nino.Core.Features.Result;

namespace Nino.Core.Features.Commands.Projects.Conga.RemoveGroupMember;

public sealed class RemoveCongaGroupMemberHandler(
    NinoDbContext db,
    IUserVerificationService verificationService,
    ILogger<RemoveCongaGroupMemberHandler> logger
) : ICommandHandler<RemoveCongaGroupMemberCommand, Result>
{
    /// <inheritdoc />
    public async Task<Result> HandleAsync(RemoveCongaGroupMemberCommand command)
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

        logger.LogInformation(
            "Removing node {NodeName} from Conga Group {GroupName} in project {ProjectId}'s Conga graph",
            command.NodeName,
            command.GroupName,
            project.Id
        );

        var result = project.CongaParticipants.RemoveGroupMember(
            command.GroupName,
            command.NodeName
        );

        return result switch
        {
            CongaModificationResult.Success => Success(),
            CongaModificationResult.NoGroup => Fail(ResultStatus.GroupNotFound),
            CongaModificationResult.NotFound => Fail(ResultStatus.NotFound),
            _ => Fail(ResultStatus.Error),
        };
    }
}
