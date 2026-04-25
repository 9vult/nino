// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using static Nino.Core.Features.Result<Nino.Core.Features.Queries.Projects.GetDebugData.GetDebugDataResponse>;

namespace Nino.Core.Features.Queries.Projects.GetDebugData;

public sealed class GetDebugDataHandler(ReadOnlyNinoDbContext db)
    : IQueryHandler<GetDebugDataQuery, Result<GetDebugDataResponse>>
{
    /// <inheritdoc />
    public async Task<Result<GetDebugDataResponse>> HandleAsync(GetDebugDataQuery query)
    {
        var result = await db
            .Projects.Where(p => p.Id == query.ProjectId)
            .Select(p => new GetDebugDataResponse(
                ProjectId: p.Id,
                GroupId: p.GroupId,
                OwnerId: p.OwnerId,
                ProjectChannelId: p.ProjectChannelId,
                Nickname: p.Nickname,
                Title: p.Title,
                AniListId: p.AniListId,
                IsPrivate: p.IsPrivate,
                IsArchived: p.IsArchived,
                EpisodeCount: p.Episodes.Count,
                TemplateStaffCount: p.TemplateStaff.Count,
                TaskCount: p.Episodes.SelectMany(e => e.Tasks).Count(),
                CongaCount: p.CongaParticipants.Nodes.Count
            ))
            .FirstOrDefaultAsync();

        return result is not null ? Success(result) : Fail(ResultStatus.ProjectNotFound);
    }
}
