using System;
using System.Net;
using System.Text;
using Discord;
using Discord.Net;
using Newtonsoft.Json;
using Nino.Records.Enums;
using NLog;
using static Localizer.Localizer;

namespace Nino.Utilities
{
    internal static class ObserverPublisher
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Publish a progress update to observers
        /// </summary>
        /// <param name="project">Project being updated</param>
        /// <param name="embed">Embed to send to observers</param>
        /// <returns></returns>
        public static async Task PublishProgress(Records.Project project, Embed embed)
        {
            var observers = Cache.GetObservers(project.GuildId).Where(o => o.ProjectId == project.Id).ToList();
            if (observers.Count != 0)
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
                        var data = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8,
                            "application/json");
                        await httpClient.PostAsync(observer.ProgressWebhook, data);
                    }
                    catch (HttpException e) when (e.HttpCode == HttpStatusCode.NotFound)
                    {
                        Log.Error($"Progress webhook for observer {observer.Id} Not Found (404)!");
                        var guild = Nino.Client.GetGuild(observer.OriginGuildId);
                        await Utils.AlertError($"An error occured while publishing to your observer: `404 NOT FOUND`. Your observer has been deleted to comply with Discord rate-limiting guidelines.", guild, project.Nickname, observer.OwnerId, "Observer/Progress");
                        
                        await AzureHelper.Observers!.DeleteItemAsync<Records.Observer>(observer.Id.ToString(), AzureHelper.ObserverPartitionKey(observer));
                        Log.Info($"Deleted observer {observer.Id} from {observer.OriginGuildId}");
                        await Cache.RebuildObserverCache();
                    }
                    catch (Exception e)
                    {
                        Log.Error($"Progress webhook for observer {observer.Id} failed: {e}");
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
        /// <param name="publishTitle">Title</param>
        /// <param name="releaseUrl">Release URL(s)</param>
        /// <returns></returns>
        public static async Task PublishRelease(Records.Project project, string publishTitle, string releaseUrl)
        {
            var observers = Cache.GetObservers(project.GuildId).Where(o => o.ProjectId == project.Id).ToList();
            if (observers.Count != 0)
            {
                var httpClient = new HttpClient();
                foreach (var observer in observers)
                {
                    if (string.IsNullOrEmpty(observer.ReleasesWebhook))
                        continue;
                    var observerRoleStr = observer.RoleId != null
                        ? observer.RoleId == observer.GuildId ? "@everyone " : $"<@&{observer.RoleId}> "
                        : "";
                    
                    var observerBody = $"**{publishTitle}**\n{observerRoleStr}{releaseUrl}";
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
                    catch (HttpException e) when (e.HttpCode == HttpStatusCode.NotFound)
                    {
                        Log.Error($"Releases webhook for observer {observer.Id} Not Found (404)!");
                        var guild = Nino.Client.GetGuild(observer.OriginGuildId);
                        await Utils.AlertError($"An error occured while publishing to your observer: `404 NOT FOUND`. Your observer has been deleted to comply with Discord rate-limiting guidelines.", guild, project.Nickname, observer.OwnerId, "Observer/Releases");
                        
                        await AzureHelper.Observers!.DeleteItemAsync<Records.Observer>(observer.Id.ToString(), AzureHelper.ObserverPartitionKey(observer));
                        Log.Info($"Deleted observer {observer.Id} from {observer.OriginGuildId}");
                        await Cache.RebuildObserverCache();
                    }
                    catch (Exception e)
                    {
                        Log.Error($"Releases webhook for observer {observer.Id} failed: {e}");
                        var guild = Nino.Client.GetGuild(observer.OriginGuildId);
                        await Utils.AlertError($"The following error occured while publishing to your observer: {e.Message}", guild, project.Nickname, observer.OwnerId, "Observer/Releases");
                    }
                }
            }
        }
    }
}
