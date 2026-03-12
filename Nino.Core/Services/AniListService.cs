// SPDX-License-Identifier: MPL-2.0

using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Nino.Core.Features;
using Nino.Domain.Dtos.AniList;
using Nino.Domain.Entities;
using Nino.Domain.ValueObjects;
using Task = System.Threading.Tasks.Task;

namespace Nino.Core.Services;

public sealed class AniListService(
    NinoDbContext db,
    HttpClient client,
    ILogger<AniListService> logger
) : IAniListService
{
    private const string BaseUrl = "https://graphql.anilist.co";
    private static readonly TimeSpan OneDay = TimeSpan.FromDays(1);

    /// <inheritdoc />
    public async Task<Result<AniListResponse>> GetAnimeAsync(AniListId aniListId)
    {
        if (aniListId.Value <= 0)
            return Result<AniListResponse>.Fail(ResultStatus.BadRequest);

        var cachedResponse = await db.AniListCache.SingleOrDefaultAsync(r => r.Id == aniListId);
        if (cachedResponse is not null && DateTimeOffset.UtcNow - cachedResponse.FetchedAt < OneDay)
            return Result<AniListResponse>.Success(cachedResponse);

        // Need to request new data
        try
        {
            var page = 1;
            var response = await client.PostAsync(BaseUrl, CreateQuery(aniListId, page));

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<AniListRoot>();
                if (data?.Data?.Media?.AiringSchedule?.Nodes is null)
                    return Result<AniListResponse>.Fail(ResultStatus.Error);

                // Round to the nearest multiple of 30 minutes
                while (page < (data?.Data?.Media?.AiringSchedule?.PageInfo?.LastPage ?? 1))
                {
                    // Sleep to try and avoid rate limiting
                    await Task.Delay(1000);

                    page++;
                    response = await client.PostAsync(BaseUrl, CreateQuery(aniListId, page));
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
                }
                else
                {
                    cachedResponse = new AniListResponse
                    {
                        Id = aniListId,
                        Data = data,
                        FetchedAt = DateTimeOffset.UtcNow,
                    };
                    await db.AniListCache.AddAsync(cachedResponse);
                }

                await db.SaveChangesAsync();
                return Result<AniListResponse>.Success(cachedResponse);
            }
            return Result<AniListResponse>.Fail(ResultStatus.Error);
        }
        catch (HttpRequestException e) when (e.StatusCode == HttpStatusCode.NotFound)
        {
            return Result<AniListResponse>.Fail(ResultStatus.NotFound);
        }
        catch (HttpRequestException e)
        {
            logger.LogError(
                e,
                "HTTP Status {Code} when getting AniList ID {AniListID}",
                e.StatusCode,
                aniListId
            );
            return Result<AniListResponse>.Fail(ResultStatus.Error);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error when getting AniList ID {AniListID}", aniListId);
            return Result<AniListResponse>.Fail(ResultStatus.Error);
        }
    }

    /// <inheritdoc />
    public async Task<Result<DateOnly>> GetAnimeStartDateAsync(AniListId aniListId)
    {
        var anime = await GetAnimeAsync(aniListId);
        if (!anime.IsSuccess)
            return Result<DateOnly>.Fail(anime.Status);

        var fuzzy = anime.Value?.Data?.Data?.Media?.StartDate;
        if (fuzzy is null || fuzzy.Year == 1)
            return Result<DateOnly>.Fail(ResultStatus.Error);

        var startDate = new DateOnly(fuzzy.Year, fuzzy.Month, fuzzy.Day);
        return Result<DateOnly>.Success(startDate);
    }

    /// <inheritdoc />
    public async Task<Result<DateTimeOffset>> GetEpisodeAirTimeAsync(
        AniListId aniListId,
        decimal episodeNumber
    )
    {
        var anime = await GetAnimeAsync(aniListId);
        if (!anime.IsSuccess)
            return Result<DateTimeOffset>.Fail(anime.Status);

        var episode = (anime.Value?.Data?.Data?.Media?.AiringSchedule?.Nodes ?? []).FirstOrDefault(
            e => e.Episode == episodeNumber
        );
        if (episode is null)
            return Result<DateTimeOffset>.Fail(ResultStatus.NotFound);

        var time = DateTimeOffset
            .FromUnixTimeSeconds(episode.AiringAt)
            .AddMinutes(anime.Value?.Data?.Data?.Media?.Duration ?? 0);
        return Result<DateTimeOffset>.Success(time);
    }

    /// <inheritdoc />
    public async Task<Result<bool>> AnimeHasStartedAsync(AniListId aniListId)
    {
        var startDate = await GetAnimeStartDateAsync(aniListId);
        if (!startDate.IsSuccess)
            return Result<bool>.Fail(startDate.Status);

        var started = startDate.Value <= DateOnly.FromDateTime(DateTime.UtcNow);
        return Result<bool>.Success(started);
    }

    /// <inheritdoc />
    public async Task<Result<bool>> EpisodeHasAiredAsync(AniListId aniListId, decimal episodeNumber)
    {
        var airTime = await GetEpisodeAirTimeAsync(aniListId, episodeNumber);
        if (airTime.IsSuccess)
        {
            var aired = airTime.Value <= DateTimeOffset.UtcNow;
            return Result<bool>.Success(aired);
        }

        if (airTime.Status == ResultStatus.NotFound)
        {
            // Estimate
            var startDate = await GetAnimeStartDateAsync(aniListId);
            if (!startDate.IsSuccess)
                return Result<bool>.Fail(startDate.Status);

            var aired =
                startDate.Value.AddDays(7 * ((int)episodeNumber - 1))
                < DateOnly.FromDateTime(DateTime.UtcNow);
            return Result<bool>.Success(aired);
        }

        return Result<bool>.Fail(airTime.Status);
    }

    /// <inheritdoc />
    public async Task<Result<List<decimal>>> GetAiredEpisodesAsync(AniListId aniListId)
    {
        var anime = await GetAnimeAsync(aniListId);
        if (!anime.IsSuccess)
            return Result<List<decimal>>.Fail(anime.Status);

        var duration = anime.Value?.Data?.Data?.Media?.Duration ?? 0;
        var episodeNumbers = (anime.Value?.Data?.Data?.Media?.AiringSchedule?.Nodes ?? [])
            .Where(e =>
                DateTimeOffset.FromUnixTimeSeconds(e.AiringAt).AddMinutes(duration)
                < DateTimeOffset.UtcNow
            )
            .Select(e => Convert.ToDecimal(e.Episode))
            .ToList();
        return Result<List<decimal>>.Success(episodeNumbers);
    }

    private static StringContent CreateQuery(AniListId aniListId, int page)
    {
        var id = aniListId.Value;
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
