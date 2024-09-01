using Discord;
using Discord.Interactions;
using Nino.Utilities;

using static Localizer.Localizer;

namespace Nino.Commands
{
    public partial class Observer
    {
        [SlashCommand("remove", "Stop observing a project on another server")]
        public async Task<RuntimeResult> Remove(
            [Summary("serverid", "SID of the server you want to observe")] string serverId,
            [Summary("project", "Project nickname")] string alias
        )
        {
            var interaction = Context.Interaction;
            var lng = interaction.UserLocale;
            var guildId = interaction.GuildId ?? 0;

            // Sanitize inputs
            var originGuildIdStr = serverId.Trim();
            alias = alias.Trim();

            // Validate observer server
            if (!ulong.TryParse(originGuildIdStr, out var originGuildId))
                return await Response.Fail(T("error.invalidServerId", lng), interaction);
            var originGuild = Nino.Client.GetGuild(originGuildId);
            if (originGuild == null)
                return await Response.Fail(T("error.noSuchServer", lng), interaction);

            // Verify project and user access
            var project = Utils.ResolveAlias(alias, interaction, observingGuildId: originGuildId);
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
            return ExecutionResult.Success;
        }
    }
}
