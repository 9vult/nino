// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nino.Core.Services;
using Nino.Domain.Enums;
using static Nino.Core.Features.Result;

namespace Nino.Core.Features.Commands.Projects.Conga.RemoveEdge;

public sealed class RemoveCongaEdgeHandler(
    NinoDbContext db,
    IUserVerificationService verificationService,
    ILogger<RemoveCongaEdgeHandler> logger
) : ICommandHandler<RemoveCongaEdgeCommand, Result>
{
    /// <inheritdoc />
    public async Task<Result> HandleAsync(RemoveCongaEdgeCommand command)
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
            "Removing {From} → {To} from project {ProjectId}'s Conga graph",
            command.From,
            command.To,
            project.Id
        );

        var result = project.CongaParticipants.RemoveEdge(command.From, command.To);
        if (result is CongaModificationResult.Success)
        {
            db.Entry(project).Property(p => p.CongaParticipants).IsModified = true;
            await db.SaveChangesAsync();
            return Success();
        }

        return result switch
        {
            CongaModificationResult.NoLink => Fail(ResultStatus.BadRequest, "noLink"),
            CongaModificationResult.NotFound => Fail(ResultStatus.BadRequest, "notFound"),
            _ => Fail(ResultStatus.Error),
        };
    }
}
