// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
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
        var page = query.Page ?? 1;
        List<GetTasksAtMeResult> results = [];
        switch (query.Type)
        {
            case AtMeType.Incomplete:
                results = await GetViaIncomplete(query);
                break;
            case AtMeType.Conga:
                results = await GetViaConga(query);
                break;
            case AtMeType.Auto:
                results = await GetViaConga(query);
                if (results.Count == 0)
                    results = await GetViaIncomplete(query);
                break;
        }
        return Result<GetTasksAtMeResponse>.Success(
            new GetTasksAtMeResponse(
                results.Skip(ItemsPerPage * (page - 1)).Take(ItemsPerPage).ToList(),
                page,
                results.Count / ItemsPerPage
            )
        );
    }

    private async Task<List<GetTasksAtMeResult>> GetViaConga(GetTasksAtMeQuery query)
    {
        var lookup = db.Projects.AsQueryable();
        if (!query.Global)
            lookup = lookup.Where(p => p.GroupId == query.GroupId);
        var projects = await lookup
            .Where(p => p.CongaParticipants.Nodes.Count > 0)
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
            airedEpisodesByAniListId[aniListId] = result.IsSuccess ? result.Value : null;
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
                var tasks = episode.Tasks.Where(e => e.AssigneeId == query.RequestedBy).ToList();
                if (tasks.Count == 0)
                    continue;

                foreach (var task in tasks)
                {
                    if (!project.CongaParticipants.TryGetNode(task.Abbreviation, out var node))
                        continue;
                    if (node.CanBeActivated(episode.Tasks.ToList()))
                    {
                        results.Add(
                            new GetTasksAtMeResult(
                                project.Id,
                                project.Nickname,
                                episode.Number,
                                task.Name,
                                task.Weight,
                                task.IsPseudo,
                                project.AniListId
                            )
                        );
                    }
                }
            }
        }
        return results;
    }

    private async Task<List<GetTasksAtMeResult>> GetViaIncomplete(GetTasksAtMeQuery query)
    {
        var lookup = db.Tasks.Where(t => t.AssigneeId == query.RequestedBy).Where(t => !t.IsDone);

        if (!query.Global)
            lookup = lookup.Where(t => t.Project.GroupId == query.GroupId);

        if (!query.IncludePseudo)
            lookup = lookup.Where(t => !t.IsPseudo);

        var assignedTasks = (
            await lookup
                .Select(t => new GetTasksAtMeResult(
                    t.ProjectId,
                    t.Project.Nickname,
                    t.Episode.Number,
                    t.Name,
                    t.Weight,
                    t.IsPseudo,
                    t.Project.AniListId
                ))
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
        return assignedTasks
            .Where(t =>
            {
                var airedEpisodes = airedEpisodesByAniListId[t.AniListId];

                if (airedEpisodes is null)
                    return true;

                return !t.EpisodeNumber.IsDecimal(out var value) || airedEpisodes.Contains(value);
            })
            .ToList();
    }
}
