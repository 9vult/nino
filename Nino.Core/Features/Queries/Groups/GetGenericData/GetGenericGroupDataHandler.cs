// SPDX-License-Identifier: MPL-2.0

using Microsoft.EntityFrameworkCore;
using static Nino.Core.Features.Result<Nino.Core.Features.Queries.Groups.GetGenericData.GetGenericGroupDataResponse>;

namespace Nino.Core.Features.Queries.Groups.GetGenericData;

public class GetGenericGroupDataHandler(ReadOnlyNinoDbContext db)
    : IQueryHandler<GetGenericGroupDataQuery, Result<GetGenericGroupDataResponse>>
{
    /// <inheritdoc />
    public async Task<Result<GetGenericGroupDataResponse>> HandleAsync(
        GetGenericGroupDataQuery query
    )
    {
        var response = await db
            .Groups.Where(g => g.Id == query.GroupId)
            .Select(g => new GetGenericGroupDataResponse(GroupId: g.Id, GroupName: g.Name))
            .FirstOrDefaultAsync();

        return response is not null ? Success(response) : Fail(ResultStatus.NotFound);
    }
}
