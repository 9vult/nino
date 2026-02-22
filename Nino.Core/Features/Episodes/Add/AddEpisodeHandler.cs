// SPDX-License-Identifier: MPL-2.0

using System.Globalization;
using System.Text.RegularExpressions;
using Nino.Core.Entities;
using Nino.Core.Enums;
using Nino.Core.Services;
using Task = Nino.Core.Entities.Task;

namespace Nino.Core.Features.Episodes.Add;

public partial class AddEpisodeHandler(
    DataContext db,
    IUserVerificationService verificationService,
    ILogger<AddEpisodeHandler> logger
)
{
    public async Task<Result<int>> HandleAsync(AddEpisodeCommand action)
    {
        var (projectId, format, quantity, requestedBy) = action;

        if (
            !await verificationService.VerifyProjectPermissionsAsync(
                projectId,
                requestedBy,
                PermissionsLevel.Administrator
            )
        )
            return new Result<int>(ResultStatus.Unauthorized);

        var project = await db
            .Projects.Include(p => p.Episodes)
            .SingleOrDefaultAsync(p => p.Id == projectId);

        if (project is null)
            return new Result<int>(ResultStatus.NotFound);

        List<Episode> newEpisodes = [];

        var match = FormatRegex().Match(format);
        if (match.Success) // The user input has a $(n) template variable
        {
            var start = decimal.Parse(match.Groups[1].Value);

            for (var n = start; n < start + quantity; n++)
            {
                var number = FormatRegex()
                    .Replace(format, n.ToString(CultureInfo.InvariantCulture));

                if (project.Episodes.Any(e => e.Number == number))
                    continue;

                newEpisodes.Add(
                    new Episode
                    {
                        ProjectId = projectId,
                        Number = number,
                        IsDone = false,
                        AirNotificationPosted = false,
                    }
                );
            }
        }
        else if ( // The user input is a decimal
            decimal.TryParse(format, NumberStyles.Any, CultureInfo.InvariantCulture, out var start)
        )
        {
            for (var n = start; n < start + quantity; n++)
            {
                var number = n.ToString(CultureInfo.InvariantCulture);
                if (project.Episodes.Any(e => e.Number == number))
                    continue;

                newEpisodes.Add(
                    new Episode
                    {
                        ProjectId = projectId,
                        Number = number,
                        IsDone = false,
                        AirNotificationPosted = false,
                    }
                );
            }
        }
        else // User input is something else
        {
            // If count isn't 1, fail since we can't calculate what the subsequent values would be
            if (quantity != 1)
                return new Result<int>(ResultStatus.BadRequest);

            if (project.Episodes.All(e => e.Number != format))
            {
                newEpisodes.Add(
                    new Episode
                    {
                        ProjectId = projectId,
                        Number = format,
                        IsDone = false,
                        AirNotificationPosted = false,
                    }
                );
            }
        }

        foreach (var episode in newEpisodes)
        {
            var tasks = project.KeyStaff.Select(s => new Task
            {
                Abbreviation = s.Role.Abbreviation,
                IsDone = false,
            });
            episode.Tasks = tasks.ToList();
            project.Episodes.Add(episode);
        }

        await db.SaveChangesAsync();
        return new Result<int>(ResultStatus.Success, newEpisodes.Count);
    }

    [GeneratedRegex(@"\$\((\d+(\.\d+)?)\)")]
    private static partial Regex FormatRegex();
}
