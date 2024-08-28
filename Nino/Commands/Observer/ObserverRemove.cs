using Discord;
using Discord.WebSocket;
using Nino.Utilities;

using static Localizer.Localizer;

namespace Nino.Commands
{
    internal static partial class Observer
    {
        public static async Task<bool> HandleRemove(SocketSlashCommand interaction)
        {
            var lng = interaction.UserLocale;
            var guildId = interaction.GuildId ?? 0;
            var subcommand = interaction.Data.Options.First();

            // Get inputs
            var originGuildIdStr = ((string)subcommand.Options.FirstOrDefault(o => o.Name == "serverid")).Trim();
            var alias = ((string)subcommand.Options.FirstOrDefault(o => o.Name == "project")!.Value).Trim();

            // Validate observer server
            if (!ulong.TryParse(originGuildIdStr, out var originGuildId))
                return await Response.Fail(T("error.invalidServerId", lng), interaction);
            var originGuild = Nino.Client.GetGuild(originGuildId);
            if (originGuild == null)
                return await Response.Fail(T("error.noSuchServer", lng), interaction);

            // Verify project and user access
            var project = await Utils.ResolveAlias(alias, interaction, observingGuildId: originGuildId);
            if (project == null)
                return await Response.Fail(T("error.alias.resolutionFailed", lng, alias), interaction);

            // Validate observer
            var observer = Cache.GetObservers(originGuildId)
                .Where(o => o.GuildId == guildId && o.ProjectId == project.Id).SingleOrDefault();

            if (observer == null)
                return await Response.Fail(T("error.noSuchObserver", lng), interaction);

            // Remove from database
            await AzureHelper.Observers!.DeleteItemAsync<Records.Observer>(observer.Id, AzureHelper.ObserverPartitionKey(observer));
            log.Info($"Deleted observer {observer.Id}");

            // Send success embed
            var embed = new EmbedBuilder()
                .WithTitle(T("title.observer", lng))
                .WithDescription(T("observer.removed", lng, project.Nickname))
                .Build();
            await interaction.FollowupAsync(embed: embed);

            await Cache.RebuildObserverCache();
            return true;
        }
    }
}
