// SPDX-License-Identifier: MPL-2.0

using Nino.Core.Entities;

namespace Nino.Core.Services;

public interface IAniListService
{
    Task<AniListResponse> GetAnimeAsync(int aniListId);
    Task<Result<DateOnly>> GetAnimeStartDateAsync(int aniListId);
    Task<Result<DateTimeOffset>> GetEpisodeAirTimeAsync(int aniListId, decimal episodeNumber);
    Task<Result<bool>> AnimeHasStartedAsync(int aniListId);
    Task<Result<bool>> EpisodeHasAiredAsync(int aniListId, decimal episodeNumber);
    Task<Result<List<decimal>>> GetAiredEpisodesAsync(int aniListId);
}
