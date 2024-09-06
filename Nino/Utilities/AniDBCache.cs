using NLog;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Nino.Utilities
{
    internal static class AniDBCache
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private const string CACHE = ".cache";

        public static bool ANIDB_ENABLED { get; set; } = true;

        private static HttpClient _client = new(new HttpClientHandler()
        {
            AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
        });

        /// <summary>
        /// Get series data from the local cache or AniDB's API
        /// </summary>
        /// <param name="anidbId">AniDB series ID</param>
        /// <returns>XML content or an error code</returns>
        public static async Task<string> Get(string anidbId)
        {
            if (!ANIDB_ENABLED)
                return "error.anidb.disabled";

            var clientId = Nino.Config.AniDbApiClientName;
            var baseUrl = $"http://api.anidb.net:9001/httpapi?client={clientId}&clientver=1&protover=1&request=anime&aid={anidbId}";

            var filename = $"{anidbId}.xml";
            var filenameFull = Path.Combine(CACHE, filename);
            PrepareCacheDirectory();

            try
            {
                // Check if the file exists and is younger than a day old
                var fileInfo = new FileInfo(filenameFull);
                if (fileInfo.Exists && (DateTime.UtcNow - fileInfo.LastAccessTimeUtc < new TimeSpan(days: 1, 0, 0, 0)))
                {
                    // Use the cached version
                    using var reader = new StreamReader(filenameFull, encoding: Encoding.UTF8);
                    return reader.ReadToEnd();
                }
            }
            catch (IOException e)
            {
                log.Error(e.Message);
                return "error.anidb.cache.ioerror";
            }

            // Either the file doesn't exist, or it's older than a day old; Time to get new info!

            try
            {
                var response = await _client.GetAsync(baseUrl);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    
                    using var writer = new StreamWriter(filenameFull, true, Encoding.UTF8);
                    writer.Write(content);

                    return content;
                }
            }
            catch (HttpRequestException e) when (e.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return "error.anidb.404";
            }
            catch (HttpRequestException e)
            {
                log.Error(e.Message);
                return "error.anidb.apiError";
            }
            catch (IOException e)
            {
                log.Error(e.Message);
                return "error.anidb.cache.ioerror";
            }

            return "error.anidb.generic";
        }

        /// <summary>
        /// Get the XML document
        /// </summary>
        /// <param name="anidbId">AniDB ID</param>
        /// <returns>XML document object</returns>
        public static async Task<XDocument?> GetXml(string anidbId)
        {
            if (!ANIDB_ENABLED)
                throw new Exception("error.anidb.disabled");

            var response = await Get(anidbId);
            try
            {
                return XDocument.Parse(response);
            }
            catch (XmlException e)
            {
                log.Error(e.Message);
                throw new Exception("error.anidb.xml");
            }
        }

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
}
