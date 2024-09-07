using System;
using System.Text;
using Discord;
using Newtonsoft.Json;
using Nino.Records.Enums;
using NLog;
using static Localizer.Localizer;

namespace Nino.Utilities
{
    internal static class ObserverPublisher
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Publish a progress update to observers
        /// </summary>
        /// <param name="project">Project being updated</param>
        /// <param name="embed">Embed to send to observers</param>
        /// <returns></returns>
        public static async Task PublishProgress(Records.Project project, Embed embed)
        {
            var observers = Cache.GetObservers(project.GuildId).Where(o => o.ProjectId == project.Id);
            if (observers.Any())
            {
                var httpClient = new HttpClient();
                foreach (var observer in observers)
                {
                    if (string.IsNullOrEmpty(observer.ProgressWebhook))
                        continue;
                    try
                    {
                        var payload = new
                        {
                            username = "Nino",
                            avatar_url = "https://i.imgur.com/PWtteaY.png",
                            content = "",
                            embeds = new[] { Utils.EmbedToJsonObject(embed) }
                        };
                        var data = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
                        await httpClient.PostAsync(observer.ProgressWebhook, data);
                    }
                    catch (Exception e)
                    {
                        log.Error($"Progress webhook for observer {observer.Id} failed: {e}");
                        var guild = Nino.Client.GetGuild(observer.OriginGuildId);
                        await Utils.AlertError($"The following error occured while publishing to your observer: {e.Message}", guild, project.Nickname, observer.OwnerId, "Observer/Progress");
                    }
                }
            }
        }

        /// <summary>
        /// Publish a release to observers
        /// </summary>
        /// <param name="project">Project being released</param>
        /// <param name="releaseType">Type of release</param>
        /// <param name="releaseNumber">Release number</param>
        /// <param name="releaseUrl">Release URL(s)</param>
        /// <returns></returns>
        public static async Task PublishRelease(Records.Project project, ReleaseType releaseType, string releaseNumber, string releaseUrl)
        {
            var observers = Cache.GetObservers(project.GuildId).Where(o => o.ProjectId == project.Id);
            if (observers.Any())
            {
                var httpClient = new HttpClient();
                foreach (var observer in observers)
                {
                    if (string.IsNullOrEmpty(observer.ReleasesWebhook))
                        continue;
                    var observerRoleStr = observer.RoleId != null
                        ? observer.RoleId == observer.GuildId ? "@everyone " : $"<@&{observer.RoleId}> "
                        : "";
                    var observerBody = releaseType != ReleaseType.Custom
                        ? $"**{project.Title} - {releaseType.ToFriendlyString("en-US")} {releaseNumber}**\n{observerRoleStr}{releaseUrl}"
                        : $"**{project.Title} - {releaseNumber}**\n{observerRoleStr}{releaseUrl}";
                    try
                    {
                        var payload = new
                        {
                            username = "Nino",
                            avatar_url = "https://i.imgur.com/PWtteaY.png",
                            content = observerBody
                        };
                        var data = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
                        await httpClient.PostAsync(observer.ReleasesWebhook, data);
                    }
                    catch (Exception e)
                    {
                        log.Error($"Releases webhook for observer {observer.Id} failed: {e}");
                        var guild = Nino.Client.GetGuild(observer.OriginGuildId);
                        await Utils.AlertError($"The following error occured while publishing to your observer: {e.Message}", guild, project.Nickname, observer.OwnerId, "Observer/Release");
                    }
                }
            }
        }
    }
}
