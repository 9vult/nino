// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using NaturalSort.Extension;
using Nino.Core.Services;
using Nino.Domain.Dtos;
using Nino.Domain.Entities;
using Nino.Domain.Enums;
using Nino.Domain.ValueObjects;
using static Nino.Core.Features.Result<System.Collections.Generic.IReadOnlyList<Nino.Core.Features.Queries.Projects.Roster.ProjectRosterStatus>>;

namespace Nino.Core.Features.Queries.Projects.Roster;

public sealed class ProjectRosterHandler(
    ReadOnlyNinoDbContext db,
    IUserVerificationService verificationService
) : IQueryHandler<ProjectRosterQuery, Result<IReadOnlyList<ProjectRosterStatus>>>
{
    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<ProjectRosterStatus>>> HandleAsync(
        ProjectRosterQuery query
    )
    {
        var verification = await verificationService.VerifyProjectPermissionsAsync(
            query.ProjectId,
            query.RequestedBy,
            PermissionsLevel.Staff
        );
        if (!verification.IsSuccess)
            return Fail(ResultStatus.Unauthorized);

        var episodes = (
            await db
                .Episodes.Where(e => e.ProjectId == query.ProjectId)
                .Select(e => new { e.Number, e.Tasks })
                .ToListAsync()
        )
            .OrderBy(e => e.Number.Value, StringComparer.OrdinalIgnoreCase.WithNaturalSort())
            .ToList();

        Dictionary<(Abbreviation, User), List<(Number, Number)>> rangeMap = [];

        for (var i = 0; i < episodes.Count; i++)
        {
            var episode = episodes[i];
            foreach (var task in episode.Tasks)
            {
                if (rangeMap.TryGetValue((task.Abbreviation, task.Assignee), out var ranges))
                {
                    var last = ranges.Last();
                    if (last.Item2 == episodes[i - 1].Number)
                    {
                        ranges[^1] = (last.Item1, episode.Number);
                        continue;
                    }
                    ranges.Add((episode.Number, episode.Number));
                    continue;
                }

                rangeMap[(task.Abbreviation, task.Assignee)] = [(episode.Number, episode.Number)];
            }
        }

        // Get a weight for each task
        var weightMap = episodes
            .SelectMany(e => e.Tasks)
            .GroupBy(t => t.Abbreviation)
            .ToDictionary(t => t.Key, t => t.First().Weight);

        // Group KVPs from the dictionary by abbreviation (merge the different users under one umbrella)
        var taskGroups = rangeMap.GroupBy(r => r.Key.Item1);

        return Success(
            taskGroups
                .Select(group => new ProjectRosterStatus(
                    Abbreviation: group.Key,
                    Assignees: group
                        .Select(g => new ProjectRosterRange(
                            MappedIdDto<UserId>.From(g.Key.Item2),
                            g.Value
                        ))
                        .ToList(),
                    Weight: weightMap[group.Key]
                ))
                .ToList()
        );
    }
}
