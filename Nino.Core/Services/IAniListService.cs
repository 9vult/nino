// SPDX-License-Identifier: MPL-2.0

using Nino.Core.Entities;

namespace Nino.Core.Services;

public interface IAniListService
{
    Task<AniListResponse> GetAnimeAsync(int aniListId);
}
