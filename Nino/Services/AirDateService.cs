using Nino.Utilities;
using NLog;
using System.Xml;
using System.Xml.Linq;

using static Localizer.Localizer;

namespace Nino.Services
{
    internal static class AirDateService
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private const string LANG = "en-US";
        private const string MIDNIGHT = "00:00";
        public static async Task<string> GetAirDateString(string aniDbId, decimal episodeNumber, string airTime = MIDNIGHT, string lng = LANG)
        {
            try
            {
                var estimated = false;
                var date = await GetAirDate(aniDbId, episodeNumber, airTime);
                if (date == null)
                { 
                    estimated = true;
                    date = await GetStartDate(aniDbId, airTime);
                    if (date == null)
                        return T("error.anidb.notSpecified", lng);

                    // estimate the date based on the episode number
                    date?.Add(new TimeSpan(days: (int)(7 * episodeNumber - 1), 0, 0, 0));
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

        public static async Task<DateTimeOffset?> GetStartDate(string aniDbId, string airTime = MIDNIGHT)
        {
            var response = await AniDBCache.Get(aniDbId);
            if (response.StartsWith("error"))
            {
                throw new Exception(response);
            }
            try
            {
                var doc = XDocument.Parse(response);

                var root = doc.Root;
                if (root?.Name.ToString() == "error")
                    throw new Exception("error.anidb.generic");

                var airDate = root?.Element("startdate")?.Value; // YYYY-MM-DD
                if (airDate != null)
                {
                    airDate = $"{airDate}T{airTime}+09:00"; // Japan
                    return DateTimeOffset.Parse(airDate);
                }

                return null;
            }
            catch (XmlException e)
            {
                log.Error(e.Message);
                throw new Exception("error.anidb.xml");
            }
        }

        public static async Task<DateTimeOffset?> GetAirDate(string aniDbId, decimal episodeNumber, string airTime = MIDNIGHT)
        {
            var response = await AniDBCache.Get(aniDbId);
            if (response.StartsWith("error"))
            {
                throw new Exception(response);
            }
            try
            {
                var doc = XDocument.Parse(response);

                var root = doc.Root;
                if (root?.Name.ToString() == "error")
                    throw new Exception("error.anidb.generic");

                var episodes = doc.Descendants("episode");
                var target = episodes.FirstOrDefault(e => e.Element("epno")?.Value == episodeNumber.ToString());

                if (target != null)
                {
                    var airDate = target.Element("airdate")?.Value; // YYYY-MM-DD
                    if (airDate != null)
                    {
                        airDate = $"{airDate}T{airTime}+09:00"; // Japan
                        return DateTimeOffset.Parse(airDate);
                    }
                }
                return null;
            }
            catch (XmlException e)
            {
                log.Error(e.Message);
                throw new Exception("error.anidb.xml");
            }
        }
    }
}
