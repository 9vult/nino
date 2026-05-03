// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using NaturalSort.Extension;
using Nino.Core.Services;
using Nino.Domain.Enums;
using Nino.Domain.ValueObjects;

namespace Nino.Core.Features.Queries.Tasks.AtMe;

public sealed class GetTasksAtMeHandler(ReadOnlyNinoDbContext db, IAniListService aniListService)
    : IQueryHandler<GetTasksAtMeQuery, Result<GetTasksAtMeResponse>>
{
    private const int ItemsPerPage = 12;

    /// <inheritdoc />
    public async Task<Result<GetTasksAtMeResponse>> HandleAsync(GetTasksAtMeQuery query)
    {
        var page = query.Page ?? 0;
        List<GetTasksAtMeResult> results = [];
        var resultingType = AtMeType.Auto;
        switch (query.Type)
        {
            case AtMeType.Incomplete:
                results = await GetViaIncomplete(query);
                resultingType = AtMeType.Incomplete;
                break;
            case AtMeType.Conga:
                results = await GetViaConga(query);
                resultingType = AtMeType.Conga;
                break;
            case AtMeType.Auto:
                results = await GetViaConga(query);
                resultingType = AtMeType.Conga;

                if (results.Count == 0)
                {
                    results = await GetViaIncomplete(query);
                    resultingType = AtMeType.Incomplete;
                }
                break;
        }

        // Order results for consistency
        var pagedResults = results
            .OrderBy(r => r.Nickname.Value)
            .ThenBy(r => r.EpisodeNumber.Value, StringComparer.OrdinalIgnoreCase.WithNaturalSort())
            .Skip(ItemsPerPage * page)
            .Take(ItemsPerPage);

        return Result<GetTasksAtMeResponse>.Success(
            new GetTasksAtMeResponse(
                pagedResults.ToList(),
                resultingType,
                page,
                (int)Math.Max(1, Math.Ceiling(results.Count / (decimal)ItemsPerPage))
            )
        );
    }

    private async Task<List<GetTasksAtMeResult>> GetViaConga(GetTasksAtMeQuery query)
    {
        var lookup = db
            .Projects.Where(p => !p.IsArchived)
            .Where(p => p.Tasks.Any(t => !t.IsDone && t.AssigneeId == query.RequestedBy));
        if (!query.Global)
            lookup = lookup.Where(p => p.GroupId == query.GroupId);
        var projects = await lookup
            .Select(p => new
            {
                p.Id,
                p.Nickname,
                p.AniListId,
                p.CongaParticipants,
                Episodes = p
                    .Episodes.Where(e => !e.IsDone)
                    .Select(e => new
                    {
                        e.Id,
                        e.Number,
                        e.Tasks,
                    }),
            })
            .ToListAsync();

        projects = projects.Where(p => p.CongaParticipants.Nodes.Count > 0).ToList();

        // Get aired episodes
        var airedEpisodesByAniListId = new Dictionary<AniListId, List<decimal>?>();
        foreach (var aniListId in projects.Select(t => t.AniListId).Distinct())
        {
            if (aniListId == AniListId.Unset)
            {
                airedEpisodesByAniListId[aniListId] = null; // null = take all
                continue;
            }

            var result = await aniListService.GetAiredEpisodesAsync(aniListId);
            airedEpisodesByAniListId[aniListId] =
                result.IsSuccess && result.Value.Count > 0 ? result.Value : null;
        }

        List<GetTasksAtMeResult> results = [];
        foreach (var project in projects)
        {
            // Restrict to only aired episodes
            var episodes = project.Episodes.Where(e =>
            {
                var airedEpisodes = airedEpisodesByAniListId[project.AniListId];
                if (airedEpisodes is null)
                    return true;
                return !e.Number.IsDecimal(out var value) || airedEpisodes.Contains(value);
            });
            foreach (var episode in episodes)
            {
                var epTasks = episode
                    .Tasks.Where(t => t.AssigneeId == query.RequestedBy && !t.IsDone)
                    .ToList();
                if (epTasks.Count == 0)
                    continue;

                List<GetTasksAtMeTaskResult> tasks = [];

                foreach (var task in epTasks)
                {
                    if (!project.CongaParticipants.TryGetNode(task.Abbreviation, out var node))
                        continue;
                    if (node.CanBeActivated(episode.Tasks.ToList()))
                    {
                        tasks.Add(
                            new GetTasksAtMeTaskResult(
                                task.Abbreviation,
                                task.Weight,
                                task.IsPseudo
                            )
                        );
                    }
                }

                if (tasks.Count > 0)
                {
                    results.Add(
                        new GetTasksAtMeResult(
                            project.Id,
                            project.Nickname,
                            episode.Number,
                            project.AniListId,
                            tasks
                        )
                    );
                }
            }
        }
        return results;
    }

    private async Task<List<GetTasksAtMeResult>> GetViaIncomplete(GetTasksAtMeQuery query)
    {
        var lookup = db
            .Tasks.Where(t => !t.Project.IsArchived)
            .Where(t => t.AssigneeId == query.RequestedBy && !t.IsDone);

        if (!query.Global)
            lookup = lookup.Where(t => t.Project.GroupId == query.GroupId);

        if (!query.IncludePseudo)
            lookup = lookup.Where(t => !t.IsPseudo);

        var assignedTasks = (
            await lookup
                .Select(t => new
                {
                    t.ProjectId,
                    t.Project.Nickname,
                    t.Episode.Number,
                    t.Abbreviation,
                    t.Weight,
                    t.IsPseudo,
                    t.Project.AniListId,
                })
                .ToListAsync()
        ).ToList();

        // Get aired episodes
        var airedEpisodesByAniListId = new Dictionary<AniListId, List<decimal>?>();
        foreach (var aniListId in assignedTasks.Select(t => t.AniListId).Distinct())
        {
            if (aniListId == AniListId.Unset)
            {
                airedEpisodesByAniListId[aniListId] = null; // null = take all
                continue;
            }

            var result = await aniListService.GetAiredEpisodesAsync(aniListId);
            airedEpisodesByAniListId[aniListId] = result.IsSuccess ? result.Value : null;
        }

        // Restrict to only aired episodes
        assignedTasks = assignedTasks
            .Where(t =>
            {
                var airedEpisodes = airedEpisodesByAniListId[t.AniListId];

                if (airedEpisodes is null)
                    return true;

                return !t.Number.IsDecimal(out var value) || airedEpisodes.Contains(value);
            })
            .ToList();

        List<GetTasksAtMeResult> results = [];
        foreach (var episode in assignedTasks.GroupBy(t => t.Number))
        {
            var number = episode.Key;
            var data = episode.First();
            var projectId = data.ProjectId;
            var nickname = data.Nickname;
            var aniListId = data.AniListId;
            var tasks = episode
                .Select(t => new GetTasksAtMeTaskResult(t.Abbreviation, t.Weight, t.IsPseudo))
                .ToList();
            results.Add(new GetTasksAtMeResult(projectId, nickname, number, aniListId, tasks));
        }
        return results;
    }
}
