// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nino.Core.Services;
using Nino.Domain.Enums;
using static Nino.Core.Features.Result;

namespace Nino.Core.Features.Commands.Projects.Conga.AddEdge;

public sealed class AddCongaEdgeHandler(
    NinoDbContext db,
    IUserVerificationService verificationService,
    ILogger<AddCongaEdgeHandler> logger
) : ICommandHandler<AddCongaEdgeCommand, Result>
{
    /// <inheritdoc />
    public async Task<Result> HandleAsync(AddCongaEdgeCommand command)
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

        var tasks = project.Episodes.SelectMany(e => e.Tasks).ToList();

        if (
            !command.From.Value.StartsWith('$')
            && !command.From.Value.StartsWith('@')
            && tasks.All(t => t.Abbreviation != command.From)
        )
            return Fail(ResultStatus.TaskNotFound, "from");

        if (
            !command.To.Value.StartsWith('$')
            && !command.To.Value.StartsWith('@')
            && tasks.All(t => t.Abbreviation != command.To)
        )
            return Fail(ResultStatus.TaskNotFound, "to");

        logger.LogInformation(
            "Adding {From} → {To} to project {ProjectId}'s Conga graph",
            command.From,
            command.To,
            project.Id
        );

        var result = project.CongaParticipants.AddEdge(command.From, command.To);
        if (result is CongaModificationResult.Success)
        {
            await db.SaveChangesAsync();
            return Success();
        }

        return result switch
        {
            CongaModificationResult.MixedGroups => Fail(ResultStatus.BadRequest, "mixedGroups"),
            CongaModificationResult.SelfLoop => Fail(ResultStatus.BadRequest, "selfLoop"),
            CongaModificationResult.Cycle => Fail(ResultStatus.BadRequest, "cycle"),
            CongaModificationResult.Duplicate => Fail(ResultStatus.CongaConflict),
            CongaModificationResult.IllegalTree => Fail(ResultStatus.BadRequest, "illegalTree"),
            _ => Fail(ResultStatus.Error),
        };
    }
}
