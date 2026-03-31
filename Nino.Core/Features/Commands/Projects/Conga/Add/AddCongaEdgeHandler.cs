// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nino.Core.Services;
using Nino.Domain.Enums;
using static Nino.Core.Features.Result;

namespace Nino.Core.Features.Commands.Projects.Conga.Add;

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

        // Validate edge doesn't already exist
        if (
            project.CongaParticipants.Contains(command.Current)
            && project
                .CongaParticipants.GetDependentsOf(command.Current)
                .Any(n => n.Abbreviation == command.Next)
        )
            return Fail(ResultStatus.CongaConflict);

        CongaNodeType currentType;
        CongaNodeType nextType;
        var tasks = project.Episodes.SelectMany(e => e.Tasks).ToList();

        if (command.Current.Value.StartsWith('$'))
            currentType = CongaNodeType.Special;
        else if (command.Current.Value.StartsWith('@'))
            currentType = CongaNodeType.Group;
        else if (tasks.Any(t => t.Abbreviation == command.Current))
            currentType = CongaNodeType.Task;
        else
            return Fail(ResultStatus.TaskNotFound, "current");

        if (command.Next.Value.StartsWith('$'))
            nextType = CongaNodeType.Special;
        else if (command.Next.Value.StartsWith('@'))
            nextType = CongaNodeType.Group;
        else if (tasks.Any(t => t.Abbreviation == command.Next))
            nextType = CongaNodeType.Task;
        else
            return Fail(ResultStatus.TaskNotFound, "next");

        logger.LogInformation(
            "Adding {Current} → {Next} to project {ProjectId}'s Conga graph",
            command.Current,
            command.Next,
            project.Id
        );

        project.CongaParticipants.Add(command.Current, command.Next, currentType, nextType);

        await db.SaveChangesAsync();
        return Success();
    }
}
