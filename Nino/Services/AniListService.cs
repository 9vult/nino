using System.Net;
using Nino.Utilities;
using NLog;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nino.Services
{
    internal static class AniListService
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private const string Cache = ".cache";
        private const string BaseUrl = "https://graphql.anilist.co";
        private static readonly TimeSpan OneDay = TimeSpan.FromDays(1);
        
        private static readonly Dictionary<int, ApiResponse> RamCache = new();
        
        public static bool AniListEnabled { get; set; } = true;

        private static readonly HttpClient Client = new(new HttpClientHandler
        {
            AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
        });

        /// <summary>
        /// Get the AniList API response for the given ID, either cached or fresh
        /// </summary>
        /// <param name="anilistId">AniList ID</param>
        /// <returns>API response</returns>
        public static async Task<ApiResponse?> Get(int anilistId)
        {
            if (!AniListEnabled)
                return new ApiResponse { Error = "error.anilist.disabled" };
            if (anilistId <= 0)
                return new ApiResponse { Error = "error.anilist.404" };

            // Check if it's in the RAM cache
            if (RamCache.TryGetValue(anilistId, out var ramValue))
            {
                if (DateTime.UtcNow - ramValue.SaveDate.ToUniversalTime() < OneDay)
                    return ramValue;
            }
            
            // It's either not in the RAM cache or it's too old, continue to the disk cache
            
            var filename = Path.Combine(Cache, $"{anilistId}.json");
            PrepareCacheDirectory();

            try
            {
                // Check if the file exists and is younger than a day old
                var fileInfo = new FileInfo(filename);
                if (fileInfo.Exists && (DateTime.UtcNow - fileInfo.LastWriteTimeUtc < OneDay))
                {
                    // Use the cached version
                    await using var stream = File.OpenRead(filename);
                    var response = await JsonSerializer.DeserializeAsync<ApiResponse>(stream);
                    
                    if (response is null) return null;
                    response.SaveDate = fileInfo.LastWriteTimeUtc;
                    RamCache[anilistId] = response;
                    return response;
                }
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
                return new ApiResponse { Error = "error.anilist.cache.ioerror" };
            }

            // Either the file doesn't exist, or it's older than a day old; Time to get new info!

            try
            {
                var response = await Client.PostAsync(BaseUrl, CreateQuery(anilistId));

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse>();
                    if (apiResponse is null)
                        return new ApiResponse { Error = "error.anilist.cache.malformed" };
                    
                    // Round to the nearest multiple of 30 minutes
                    if (apiResponse.Data?.Media is not null)
                        apiResponse.Data.Media.Duration = (int)Math.Ceiling((apiResponse.Data.Media.Duration ?? 0) / 30d) * 30;

                    await using var stream = File.OpenWrite(filename);
                    await JsonSerializer.SerializeAsync(stream, apiResponse);
                    
                    apiResponse.SaveDate = DateTime.UtcNow;
                    RamCache[anilistId] = apiResponse;
                    return apiResponse;
                }
                
                Log.Error($"AniList status code for ID {anilistId} is {response.StatusCode}");
                return new ApiResponse
                {
                    Error =
                        response.StatusCode == HttpStatusCode.NotFound
                            ? $"error.anilist.404"
                            : $"error.anilist.generic",
                };

            }
            catch (HttpRequestException e) when (e.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new ApiResponse
                {
                    Error = "error.anilist.404"
                };
            }
            catch (HttpRequestException e)
            {
                Log.Error($"Error getting Start Date for AniListId: {anilistId}");
                Log.Error(e.Message);
                return new ApiResponse
                {
                    Error = "error.anilist.apiError"
                };
            }
            catch (Exception e)
            {
                Log.Error($"Error getting Start Date for AniListId: {anilistId}");
                Log.Error(e.Message);
                return new ApiResponse { Error = $"error.anilist.generic" };
            }
        }

        private static StringContent CreateQuery(int id) =>
            new(JsonSerializer.Serialize(new
            {
                query = """
                        query ($id: Int) {
                            Media (id: $id) {
                                startDate { year month day },
                                airingSchedule { nodes { episode, airingAt }},
                                duration
                            }
                        }
                        """,
                variables = new { id },
            }),
            Encoding.UTF8,
            "application/json");

        /// <summary>
        /// Prepare the cache directory
        /// </summary>
        private static void PrepareCacheDirectory()
        {
            try
            {
                if (Directory.Exists(Cache)) return;
                Directory.CreateDirectory(Cache);
            }
            catch (IOException e)
            {
                Log.Error(e.Message);
            }
        }
    }

    public class ApiResponse
    {
        public Data? Data { get; init; }

        [JsonIgnore]
        public List<AiringScheduleNode>? Episodes => Data?.Media?.AiringSchedule?.Nodes;
        [JsonIgnore]
        public DateTimeOffset? StartDate => Data?.Media?.StartDate;
        [JsonIgnore]
        public int? Duration => Data?.Media?.Duration; 
        [JsonIgnore]
        public string? Error { get; set; }
        [JsonIgnore]
        public DateTime SaveDate { get; set; }
    }

    public class Data
    {
        public Media? Media { get; set; }
    }

    public class Media
    {
        public AiringSchedule? AiringSchedule { get; set; }

        [JsonConverter(typeof(StartDateConverter))]
        public DateTimeOffset StartDate { get; set; }
        public int? Duration { get; set; }
    }

    public class AiringSchedule
    {
        public List<AiringScheduleNode>? Nodes { get; set; }
    }

    public class AiringScheduleNode
    {
        public int Episode { get; set; }
        public long AiringAt { get; set; }
    }
}
