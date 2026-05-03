// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using static Nino.Core.Features.Result<Nino.Core.Features.Queries.Projects.GetDebugData.GetProjectDebugDataResponse>;

namespace Nino.Core.Features.Queries.Projects.GetDebugData;

public sealed class GetProjectDebugDataHandler(ReadOnlyNinoDbContext db)
    : IQueryHandler<GetProjectDebugDataQuery, Result<GetProjectDebugDataResponse>>
{
    /// <inheritdoc />
    public async Task<Result<GetProjectDebugDataResponse>> HandleAsync(
        GetProjectDebugDataQuery query
    )
    {
        var result = await db
            .Projects.Where(p => p.Id == query.ProjectId)
            .Select(p => new GetProjectDebugDataResponse(
                ProjectId: p.Id,
                GroupId: p.GroupId,
                OwnerId: p.OwnerId,
                ProjectChannelId: p.ProjectChannelId,
                UpdateChannelId: p.UpdateChannelId,
                ReleaseChannelId: p.ReleaseChannelId,
                Nickname: p.Nickname,
                Title: p.Title,
                AniListId: p.AniListId,
                AniListOffset: p.AniListOffset,
                IsPrivate: p.IsPrivate,
                IsArchived: p.IsArchived,
                EpisodeCount: p.Episodes.Count,
                TemplateStaffCount: p.TemplateStaff.Count,
                TaskCount: p.Episodes.SelectMany(e => e.Tasks).Count(),
                CongaCount: p.CongaParticipants.Nodes.Count,
                ObserverCount: p.Observers.Count
            ))
            .FirstOrDefaultAsync();

        return result is not null ? Success(result) : Fail(ResultStatus.ProjectNotFound);
    }
}
