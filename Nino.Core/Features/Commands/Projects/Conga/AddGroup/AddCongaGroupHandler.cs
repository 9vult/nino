// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nino.Core.Services;
using Nino.Domain.Enums;
using Nino.Domain.ValueObjects;
using static Nino.Core.Features.Result;

namespace Nino.Core.Features.Commands.Projects.Conga.AddGroup;

public sealed class AddCongaGroupHandler(
    NinoDbContext db,
    IUserVerificationService verificationService,
    ILogger<AddCongaGroupHandler> logger
) : ICommandHandler<AddCongaGroupCommand, Result>
{
    /// <inheritdoc />
    public async Task<Result> HandleAsync(AddCongaGroupCommand command)
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

        var name = Abbreviation.From('@' + command.Name.Value.TrimStart('@'));

        logger.LogInformation(
            "Adding Group {GroupName} to project {ProjectId}'s Conga graph",
            name,
            project.Id
        );

        var result = project.CongaParticipants.AddGroup(name);
        if (result is CongaModificationResult.Success)
        {
            db.Entry(project).Property(p => p.CongaParticipants).IsModified = true;
            await db.SaveChangesAsync();
            return Success();
        }

        return result switch
        {
            CongaModificationResult.Duplicate => Fail(ResultStatus.CongaConflict),
            _ => Fail(ResultStatus.Error),
        };
    }
}
