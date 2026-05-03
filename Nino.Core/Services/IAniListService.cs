// SPDX-License-Identifier: MPL-2.0

using Nino.Core.Features;
using Nino.Domain.Entities;
using Nino.Domain.ValueObjects;
using Task = Nino.Domain.Entities.Task;

namespace Nino.Core.Services;

public interface IAniListService
{
    /// <summary>
    /// Get details about an anime
    /// </summary>
    /// <param name="aniListId">AniList ID</param>
    /// <returns>Details about the anime</returns>
    Task<Result<AniListResponse>> GetAnimeAsync(AniListId aniListId);

    /// <summary>
    /// Get the date an anime began airing
    /// </summary>
    /// <param name="aniListId">AniList ID</param>
    /// <returns>Date the first episode aired</returns>
    Task<Result<DateOnly>> GetAnimeStartDateAsync(AniListId aniListId);

    /// <summary>
    /// Get the date and time an episode is scheduled to air at
    /// </summary>
    /// <param name="aniListId">AniList ID</param>
    /// <param name="episodeNumber">Episode to check</param>
    /// <returns>Date and time the episode airs at</returns>
    Task<Result<DateTimeOffset>> GetEpisodeAirTimeAsync(AniListId aniListId, decimal episodeNumber);

    /// <summary>
    /// Check if an anime has started airing
    /// </summary>
    /// <param name="aniListId">AniList ID</param>
    /// <returns><see langword="true"/> if the first episode has aired</returns>
    Task<Result<bool>> AnimeHasStartedAsync(AniListId aniListId);

    /// <summary>
    /// Check if an episode has aired
    /// </summary>
    /// <param name="aniListId">AniList ID</param>
    /// <param name="episodeNumber">episode to check</param>
    /// <returns><see langword="true"/> if the episode has aired</returns>
    Task<Result<bool>> EpisodeHasAiredAsync(AniListId aniListId, decimal episodeNumber);

    /// <summary>
    /// Get a list of episodes that have aired
    /// </summary>
    /// <param name="aniListId">AniList ID</param>
    /// <returns>List of episodes that have already aired</returns>
    Task<Result<List<decimal>>> GetAiredEpisodesAsync(AniListId aniListId);

    /// <summary>
    /// Force update an anime, bypassing the cache expiration scheme
    /// </summary>
    /// <param name="aniListId">AniList ID</param>
    /// <returns>Details about the anime</returns>
    Task<Result<AniListResponse>> ForceUpdateAnimeAsync(AniListId aniListId);
}
