// SPDX-License-Identifier: MPL-2.0

using System.Globalization;
using Nino.Core.Services;
using Nino.Domain;
using Nino.Domain.Entities;
using Nino.Domain.Enums;
using Task = Nino.Domain.Entities.Task;

namespace Nino.Core.Features.Commands.Episodes.Add;

public sealed class AddEpisodeHandler(
    NinoDbContext db,
    IUserVerificationService verificationService,
    ILogger<AddEpisodeHandler> logger
)
{
    public async Task<Result<AddEpisodeResponse>> HandleAsync(AddEpisodeCommand input)
    {
        // Validate input
        if (decimal.TryParse(input.First, out _) && input.Count != 1)
            return Result<AddEpisodeResponse>.Fail(ResultStatus.BadRequest, "not-number");
        if (input.Format.IndexOf('$') < 0)
            return Result<AddEpisodeResponse>.Fail(ResultStatus.BadRequest, "bad-format");
        if (
            decimal.TryParse(input.First, out var f)
            && input
                .Format.Replace("$", (f + input.Count).ToString(CultureInfo.InvariantCulture))
                .Length > Length.EpisodeNumber
        )
            return Result<AddEpisodeResponse>.Fail(ResultStatus.BadRequest, "too-long");

        if (
            !await verificationService.VerifyProjectPermissionsAsync(
                input.ProjectId,
                input.RequestedBy,
                PermissionsLevel.Administrator
            )
        )
            return Result<AddEpisodeResponse>.Fail(ResultStatus.Unauthorized);

        var project = await db
            .Projects.Include(p => p.Episodes)
            .SingleAsync(p => p.Id == input.ProjectId);

        List<Episode> newEpisodes = [];

        if (decimal.TryParse(input.First, out var first))
        {
            for (var n = first; n < first + input.Count; n++)
            {
                var number = input.Format.Replace("$", n.ToString(CultureInfo.InvariantCulture));
                if (project.Episodes.Any(e => e.Number == number))
                    continue;

                newEpisodes.Add(
                    new Episode
                    {
                        ProjectId = project.Id,
                        GroupId = project.GroupId,
                        Number = number,
                        IsDone = false,
                    }
                );
            }
        }
        else
        {
            if (project.Episodes.All(e => e.Number != input.First))
            {
                newEpisodes.Add(
                    new Episode
                    {
                        ProjectId = project.Id,
                        GroupId = project.GroupId,
                        Number = input.First,
                        IsDone = false,
                    }
                );
            }
        }

        if (newEpisodes.Count == 0)
            return Result<AddEpisodeResponse>.Fail(ResultStatus.Conflict);

        foreach (var episode in newEpisodes)
        {
            var tasks = project.KeyStaff.Select(s => new Task
            {
                EpisodeId = episode.Id,
                Abbreviation = s.Role.Abbreviation,
                IsDone = false,
            });
            episode.Tasks = tasks.ToList();
            project.Episodes.Add(episode);
        }

        logger.LogInformation("Added {Count} episodes to {Project}", newEpisodes.Count, project);
        await db.SaveChangesAsync();

        return Result<AddEpisodeResponse>.Success(
            new AddEpisodeResponse(
                project.Title,
                project.Type,
                project.PosterUrl,
                AddedEpisodeCount: newEpisodes.Count
            )
        );
    }
}
