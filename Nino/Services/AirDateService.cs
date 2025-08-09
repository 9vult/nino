using NLog;
using static Localizer.Localizer;

namespace Nino.Services
{
    internal static class AirDateService
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private const string Lang = "en-US";

        public static async Task<string> GetAirDateString(int aniListId, decimal episodeNumber, string lng = Lang)
        {
            try
            {
                var estimated = false;
                var date = await GetAirDate(aniListId, episodeNumber);
                if (date is null)
                { 
                    estimated = true;

                    // estimate based on "previous" episode
                    var previousNumber = Math.Floor(episodeNumber) == episodeNumber ? episodeNumber - 1 : Math.Floor(episodeNumber);
                    date = await GetAirDate(aniListId, previousNumber);
                    if (date is not null)
                    {
                        date = date?.Add(new TimeSpan(days: 7, 0, 0, 0)); // Add a week
                    }
                    else
                    {
                        // estimate the date based on the episode number and series start date
                        date = await GetStartDate(aniListId);
                        if (date is null)
                            return T("error.anilist.notSpecified", lng);
                        date = date?.Add(new TimeSpan(days: (int)(7 * (episodeNumber - 1)), 0, 0, 0));
                    }
                }

                var future = date > DateTimeOffset.UtcNow;
                var result = string.Empty;
                var utcSeconds = date!.Value.ToUnixTimeSeconds();
                if (estimated)
                    result = T("airDate.estimatedHeader", lng) + " ";

                var absolute = $"<t:{utcSeconds}:D>";
                var relative = $"<t:{utcSeconds}:R>";

                if (future)
                    result += T("airDate.future", lng, absolute, relative);
                else
                    result += T("airDate.past", lng, absolute, relative);
                
                return result;
            }
            catch (Exception e)
            {
                return T(e.Message, lng);
            }
        }

        public static async Task<DateTimeOffset?> GetStartDate(int aniListId)
        {
            var response = await AniListService.Get(aniListId);
            if (!string.IsNullOrEmpty(response?.Error))
            {
                throw new Exception(response.Error);
            }
            // Return null if the year is 1 (complete fallback result)
            return response?.StartDate?.Year == 1 ? null : response?.StartDate;
        }

        public static async Task<bool?> EpisodeAired(int aniListId, decimal episodeNumber)
        {
            var time = await GetAirDate(aniListId, episodeNumber);
            if (time is not null) return time < DateTimeOffset.UtcNow;
            
            // Estimation
            var start = await GetStartDate(aniListId);
            if (start is null) return null; // ???
            
            return start.Value.AddDays(7 * ((double)episodeNumber - 1)) < DateTimeOffset.UtcNow;
        }

        public static async Task<List<decimal>?> AiredEpisodes(int aniListId)
        {
            var response = await AniListService.Get(aniListId);
            if (!string.IsNullOrEmpty(response?.Error))
            {
                throw new Exception(response.Error);
            }

            if (response?.Episodes?.Count == 0)
                return null;

            return (response!.Episodes ?? [])
                .Where(e => DateTimeOffset.FromUnixTimeSeconds(e.AiringAt) <= DateTimeOffset.UtcNow)
                .Select(e => Convert.ToDecimal(e.Episode))
                .ToList();
        }

        public static async Task<DateTimeOffset?> GetAirDate(int aniListId, decimal episodeNumber)
        {
            var response = await AniListService.Get(aniListId);
            if (!string.IsNullOrEmpty(response?.Error))
            {
                throw new Exception(response.Error);
            }

            foreach (var episode in response?.Episodes ?? [])
            {
                if (episode.Episode == episodeNumber)
                {
                    return DateTimeOffset.FromUnixTimeSeconds(episode.AiringAt).AddMinutes(response?.Duration ?? 0);
                }
            }
            return null;
        }
    }
}
