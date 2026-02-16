// SPDX-License-Identifier: MPL-2.0

using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Nino.Core.Dtos;
using Nino.Core.Entities;
using Nino.Core.Enums;

namespace Nino.Core.Services;

public class AniListService(DataContext db, HttpClient client, ILogger<AniListService> logger)
    : IAniListService
{
    private const string BaseUrl = "https://graphql.anilist.co";
    private const string FallbackPosterUrl = "https://files.catbox.moe/j3qizm.png";

    private static readonly TimeSpan OneDay = TimeSpan.FromDays(1);

    /// <inheritdoc />
    public async Task<AniListResponse> GetAnimeAsync(int aniListId)
    {
        var cachedResponse = await db.AniListCache.SingleOrDefaultAsync(r =>
            r.AniListId == aniListId
        );

        if (cachedResponse is not null && DateTimeOffset.UtcNow - cachedResponse.FetchedAt < OneDay)
            return cachedResponse;

        // Need to fetch
        try
        {
            var page = 1;
            var response = await client.PostAsync(BaseUrl, CreateQuery(aniListId, page));

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<AniListRoot>();
                if (data?.Data?.Media?.AiringSchedule?.Nodes is null)
                    return new AniListResponse
                    {
                        Status = ResultStatus.Error,
                        AniListId = aniListId,
                        Data = null,
                    };

                // Round to the nearest multiple of 30 minutes
                while (page < (data?.Data?.Media?.AiringSchedule?.PageInfo?.LastPage ?? 1))
                {
                    // Sleep to try and avoid rate limiting
                    await System.Threading.Tasks.Task.Delay(1000);

                    page++;
                    response = await client.PostAsync(
                        FallbackPosterUrl,
                        CreateQuery(aniListId, page)
                    );
                    if (response.IsSuccessStatusCode)
                    {
                        var innerData = await response.Content.ReadFromJsonAsync<AniListRoot>();
                        if (innerData?.Data?.Media?.AiringSchedule?.Nodes is null)
                            continue;

                        data!.Data!.Media!.AiringSchedule!.Nodes?.AddRange(
                            innerData.Data.Media.AiringSchedule.Nodes
                        );
                    }
                }

                if (cachedResponse is not null)
                {
                    cachedResponse.Data = data;
                    cachedResponse.FetchedAt = DateTimeOffset.UtcNow;
                    cachedResponse.Status = ResultStatus.Success;
                }
                else
                {
                    cachedResponse = new AniListResponse
                    {
                        AniListId = aniListId,
                        Data = data,
                        FetchedAt = DateTimeOffset.UtcNow,
                        Status = ResultStatus.Success,
                    };
                    await db.AniListCache.AddAsync(cachedResponse);
                }

                await db.SaveChangesAsync();
                return cachedResponse;
            }
            return new AniListResponse
            {
                Status = ResultStatus.NotFound,
                AniListId = aniListId,
                Data = null,
            };
        }
        catch (HttpRequestException e) when (e.StatusCode == HttpStatusCode.NotFound)
        {
            return new AniListResponse
            {
                Status = ResultStatus.NotFound,
                AniListId = aniListId,
                Data = null,
            };
        }
        catch (HttpRequestException e)
        {
            logger.LogError(
                e,
                "HTTP Status {Code} when getting AniList ID {AniListID}",
                e.StatusCode,
                aniListId
            );
            return new AniListResponse
            {
                Status = ResultStatus.Error,
                AniListId = aniListId,
                Data = null,
            };
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error when getting AniList ID {AniListID}", aniListId);
            return new AniListResponse
            {
                Status = ResultStatus.Error,
                AniListId = aniListId,
                Data = null,
            };
        }
    }

    /// <inheritdoc />
    public async Task<Result<DateOnly>> GetAnimeStartDateAsync(int aniListId)
    {
        var anime = await GetAnimeAsync(aniListId);
        if (anime.Status is not ResultStatus.Success)
            return new Result<DateOnly>(anime.Status);

        var fuzzy = anime.Data?.Data?.Media?.StartDate;
        if (fuzzy is null || fuzzy.Year == 1)
            return new Result<DateOnly>(ResultStatus.Error);

        var startDate = new DateOnly(fuzzy.Year, fuzzy.Month, fuzzy.Day);
        return new Result<DateOnly>(ResultStatus.Success, startDate);
    }

    /// <inheritdoc />
    public async Task<Result<DateTimeOffset>> GetEpisodeAirTimeAsync(
        int aniListId,
        decimal episodeNumber
    )
    {
        var anime = await GetAnimeAsync(aniListId);
        if (anime.Status is not ResultStatus.Success)
            return new Result<DateTimeOffset>(anime.Status);

        var episode = (anime.Data?.Data?.Media?.AiringSchedule?.Nodes ?? []).FirstOrDefault(e =>
            e.Episode == episodeNumber
        );
        if (episode is null)
            return new Result<DateTimeOffset>(ResultStatus.NotFound);

        var time = DateTimeOffset
            .FromUnixTimeSeconds(episode.AiringAt)
            .AddMinutes(anime.Data?.Data?.Media?.Duration ?? 0);
        return new Result<DateTimeOffset>(ResultStatus.Success, time);
    }

    /// <inheritdoc />
    public async Task<Result<bool>> AnimeHasStartedAsync(int aniListId)
    {
        var (status, startDate) = await GetAnimeStartDateAsync(aniListId);
        if (status != ResultStatus.Success)
            return new Result<bool>(status);

        var started = startDate <= DateOnly.FromDateTime(DateTime.UtcNow);
        return new Result<bool>(ResultStatus.Success, started);
    }

    /// <inheritdoc />
    public async Task<Result<bool>> EpisodeHasAiredAsync(int aniListId, decimal episodeNumber)
    {
        var (airStatus, airTime) = await GetEpisodeAirTimeAsync(aniListId, episodeNumber);
        if (airStatus == ResultStatus.Success)
        {
            var aired = airTime <= DateTimeOffset.UtcNow;
            return new Result<bool>(airStatus, aired);
        }

        if (airStatus == ResultStatus.NotFound)
        {
            // Estimate
            var (startStatus, startDate) = await GetAnimeStartDateAsync(aniListId);
            if (startStatus != ResultStatus.Success)
                return new Result<bool>(startStatus);

            var aired =
                startDate.AddDays(7 * ((int)episodeNumber - 1))
                < DateOnly.FromDateTime(DateTime.UtcNow);
            return new Result<bool>(ResultStatus.Success, aired);
        }

        return new Result<bool>(airStatus);
    }

    /// <inheritdoc />
    public async Task<Result<List<decimal>>> GetAiredEpisodesAsync(int aniListId)
    {
        var anime = await GetAnimeAsync(aniListId);
        if (anime.Status is not ResultStatus.Success)
            return new Result<List<decimal>>(anime.Status);

        var duration = anime.Data?.Data?.Media?.Duration ?? 0;
        var episodeNumbers = (anime.Data?.Data?.Media?.AiringSchedule?.Nodes ?? [])
            .Where(e =>
                DateTimeOffset.FromUnixTimeSeconds(e.AiringAt).AddMinutes(duration)
                < DateTimeOffset.UtcNow
            )
            .Select(e => Convert.ToDecimal(e.Episode))
            .ToList();
        return new Result<List<decimal>>(ResultStatus.Success, episodeNumbers);
    }

    private static StringContent CreateQuery(int id, int page)
    {
        return new StringContent(
            JsonSerializer.Serialize(
                new
                {
                    query = """
                    query ($id: Int, $page: Int) {
                        Media (id: $id) {
                            startDate { year month day },
                            airingSchedule(page: $page) {
                                pageInfo { lastPage },
                                nodes { episode, airingAt }
                            },
                            duration,
                            episodes,
                            format,
                            title {
                              romaji
                            }
                            coverImage {
                              extraLarge
                            }
                        }
                    }
                    """,
                    variables = new { id, page },
                }
            ),
            Encoding.UTF8,
            "application/json"
        );
    }
}
