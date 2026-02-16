// SPDX-License-Identifier: MPL-2.0

using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Nino.Core.Actions;
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
