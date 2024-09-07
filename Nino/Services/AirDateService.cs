using NLog;
using static Localizer.Localizer;

namespace Nino.Services
{
    internal static class AirDateService
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private const string LANG = "en-US";

        public static async Task<string> GetAirDateString(int aniListId, decimal episodeNumber, string lng = LANG)
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
                    date = await GetAirDate(aniListId, episodeNumber);
                    if (date is not null)
                    {
                        date?.Add(new TimeSpan(days: 7, 0, 0, 0)); // Add a week
                    }
                    else
                    {
                        // estimate the date based on the episode number and series start date
                        date = await GetStartDate(aniListId);
                        if (date is null)
                            return T("error.anilist.notSpecified", lng);
                        date?.Add(new TimeSpan(days: (int)(7 * episodeNumber - 1), 0, 0, 0));
                    }
                }

                var future = date > DateTimeOffset.Now;
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
            return response?.StartDate;
        }

        public static async Task<bool> EpisodeAired(int aniListId, decimal episodeNumber)
        {
            var time = await GetAirDate(aniListId, episodeNumber);
            return time < DateTimeOffset.Now;
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
                    return DateTimeOffset.FromUnixTimeSeconds(episode.AiringAt);
                }
            }
            return null;
        }
    }
}
