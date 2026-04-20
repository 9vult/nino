// SPDX-License-Identifier: MPL-2.0

using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nino.Core.Services;
using Nino.Domain.Entities.Conga;
using Nino.Domain.Enums;
using Nino.Domain.ValueObjects;
using static Nino.Core.Features.Result;

namespace Nino.Core.Features.Commands.Projects.Conga.Import;

public partial class ImportCongaHandler(
    NinoDbContext db,
    IUserVerificationService verificationService,
    ILogger<ImportCongaHandler> logger
) : ICommandHandler<ImportCongaCommand, Result>
{
    /// <inheritdoc />
    public async Task<Result> HandleAsync(ImportCongaCommand command)
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

        logger.LogInformation("Importing Conga Graph for project {ProjectId}", project.Id);

        var g = new CongaGraph();

        foreach (var line in command.Data.Split(Environment.NewLine))
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var entry = line.Trim();
            var edgeMatch = EdgeRegex().Match(entry);
            var groupMemberMatch = GroupMemberRegex().Match(entry);

            if (edgeMatch.Success)
            {
                var from = Abbreviation.From(edgeMatch.Groups[1].Value);
                var to = Abbreviation.From(edgeMatch.Groups[2].Value);
                g.AddEdge(from, to); // Ignore errors?
            }
            else if (groupMemberMatch.Success)
            {
                var group = Abbreviation.From(groupMemberMatch.Groups[1].Value);
                var node = Abbreviation.From(groupMemberMatch.Groups[2].Value);
                g.AddGroupMember(group, node); // Ignore errors?
            }
        }

        project.CongaParticipants = g;

        db.Entry(project).Property(p => p.CongaParticipants).IsModified = true;
        await db.SaveChangesAsync();
        return Success();
    }

    [GeneratedRegex("^(.+)->(.+)$")]
    private static partial Regex EdgeRegex();

    [GeneratedRegex("^(.+)+(.+)$")]
    private static partial Regex GroupMemberRegex();
}
