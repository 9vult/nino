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
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private const string CACHE = ".cache";
        private const string BASE_URL = "https://graphql.anilist.co";
        public static bool ANILIST_ENABLED { get; set; } = true;

        private static readonly HttpClient _client = new(new HttpClientHandler()
        {
            AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
        });

        /// <summary>
        /// Get the AniList API response for the given ID, either cached or fresh
        /// </summary>
        /// <param name="anilistId">AniList ID</param>
        /// <returns>API reponse</returns>
        public static async Task<ApiResponse?> Get(int anilistId)
        {
            if (!ANILIST_ENABLED)
                return new() { Error = "error.anilist.disabled" };

            var filename = Path.Combine(CACHE, $"{anilistId}.json");
            PrepareCacheDirectory();

            try
            {
                // Check if the file exists and is younger than a day old
                var fileInfo = new FileInfo(filename);
                if (fileInfo.Exists && (DateTime.UtcNow - fileInfo.LastAccessTimeUtc < new TimeSpan(days: 1, 0, 0, 0)))
                {
                    // Use the cached version
                    using var stream = File.OpenRead(filename);
                    return await JsonSerializer.DeserializeAsync<ApiResponse>(stream);
                }
            }
            catch (Exception e)
            {
                log.Error(e.Message);
                return new() { Error = "error.anilist.cache.ioerror" };
            }

            // Either the file doesn't exist, or it's older than a day old; Time to get new info!

            try
            {
                var response = await _client.PostAsync(BASE_URL, CreateQuery(anilistId));

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse>();
                    if (apiResponse is null)
                        return new() { Error = "error.anilist.cache.malformed" };

                    using var stream = File.OpenWrite(filename);
                    await JsonSerializer.SerializeAsync(stream, apiResponse);
                    return apiResponse;
                }
            }
            catch (HttpRequestException e) when (e.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new() { Error = "error.anilist.404" };
            }
            catch (HttpRequestException e)
            {
                log.Error(e.Message);
                return new() { Error = "error.anilist.apiError" };
            }
            return new() { Error = $"error.anilist.generic" };
        }

        private static StringContent CreateQuery(int id) =>
            new(JsonSerializer.Serialize(new
            {
                query = "query ($id: Int) { Media (id: $id) { airingSchedule { nodes { episode, airingAt }}}}",
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
                if (Directory.Exists(CACHE)) return;
                Directory.CreateDirectory(CACHE);
            }
            catch (IOException e)
            {
                log.Error(e.Message);
            }
        }
    }

    public class ApiResponse
    {
        public Data? Data { get; set; }

        [JsonIgnore]
        public List<AiringScheduleNode>? Episodes => Data?.Media?.AiringSchedule?.Nodes;
        [JsonIgnore]
        public DateTimeOffset? StartDate => Data?.Media?.StartDate;


        [JsonIgnore]
        public string? Error { get; set; }
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
