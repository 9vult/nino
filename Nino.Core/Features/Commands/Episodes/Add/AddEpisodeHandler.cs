// SPDX-License-Identifier: MPL-2.0

using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nino.Core.Services;
using Nino.Domain;
using Nino.Domain.Entities;
using Nino.Domain.Enums;
using Nino.Domain.ValueObjects;
using static Nino.Core.Features.Result<Nino.Core.Features.Commands.Episodes.Add.AddEpisodeResponse>;
using Task = Nino.Domain.Entities.Task;

namespace Nino.Core.Features.Commands.Episodes.Add;

public sealed class AddEpisodeHandler(
    NinoDbContext db,
    IUserVerificationService verificationService,
    ILogger<AddEpisodeHandler> logger
) : ICommandHandler<AddEpisodeCommand, Result<AddEpisodeResponse>>
{
    /// <inheritdoc />
    public async Task<Result<AddEpisodeResponse>> HandleAsync(AddEpisodeCommand command)
    {
        // Validate input
        if (!command.First.IsDecimal(out _) && command.Count != 1)
            return Fail(ResultStatus.BadRequest, "not-number");
        if (command.Format.IndexOf('$') < 0)
            return Fail(ResultStatus.BadRequest, "bad-format");
        if (
            command.First.IsDecimal(out var f)
            && command
                .Format.Replace("$", (f + command.Count).ToString(CultureInfo.InvariantCulture))
                .Length > Length.Number
        )
            return Fail(ResultStatus.BadRequest, "too-long");

        var verification = await verificationService.VerifyProjectPermissionsAsync(
            command.ProjectId,
            command.RequestedBy,
            PermissionsLevel.Administrator
        );
        if (!verification.IsSuccess)
            return Fail(verification.Status);

        var project = await db
            .Projects.Where(p => p.Id == command.ProjectId)
            .Select(p => new
            {
                p.GroupId,
                p.TemplateStaff,
                EpisodeNumbers = p.Episodes.Select(e => e.Number).ToList(),
            })
            .FirstAsync();

        List<Episode> newEpisodes = [];

        if (command.First.IsDecimal(out var first))
        {
            for (var n = first; n < first + command.Count; n++)
            {
                var formatted = command.Format.Replace(
                    "$",
                    n.ToString(CultureInfo.InvariantCulture)
                );
                var number = Number.From(formatted);
                if (project.EpisodeNumbers.Contains(number))
                    continue;

                newEpisodes.Add(
                    new Episode
                    {
                        ProjectId = command.ProjectId,
                        GroupId = project.GroupId,
                        Number = number,
                        IsDone = false,
                    }
                );
            }
        }
        else
        {
            if (!project.EpisodeNumbers.Contains(command.First))
            {
                newEpisodes.Add(
                    new Episode
                    {
                        ProjectId = command.ProjectId,
                        GroupId = project.GroupId,
                        Number = command.First,
                        IsDone = false,
                    }
                );
            }
        }

        if (newEpisodes.Count == 0)
            return Fail(ResultStatus.EpisodeConflict);

        foreach (var episode in newEpisodes)
        {
            var tasks = project.TemplateStaff.Select(s => new Task
            {
                ProjectId = episode.ProjectId,
                EpisodeId = episode.Id,
                AssigneeId = s.AssigneeId,
                Abbreviation = s.Abbreviation,
                Name = s.Name,
                Weight = s.Weight,
                IsPseudo = s.IsPseudo,
                IsDone = false,
            });
            episode.Tasks = tasks.ToList();
        }

        logger.LogInformation(
            "Adding {Count} episodes to project {ProjectId}",
            newEpisodes.Count,
            command.ProjectId
        );

        await db.Episodes.AddRangeAsync(newEpisodes);

        await db.SaveChangesAsync();
        return Success(new AddEpisodeResponse(newEpisodes.Count));
    }
}
