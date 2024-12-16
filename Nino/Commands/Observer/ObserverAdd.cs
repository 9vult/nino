using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Nino.Utilities;

using static Localizer.Localizer;

namespace Nino.Commands
{
    public partial class Observer
    {
        [SlashCommand("add", "Start observing a project on another server")]
        public async Task<RuntimeResult> Add(
            [Summary("serverid", "ID of the server you want to observe")] string serverId,
            [Summary("project", "Project nickname")] string alias,
            [Summary("blame", "Should this project's aliases show up in /blame?")] bool blame,
            [Summary("updates", "Webhook URL for progress updates")] string? updatesUrl = null,
            [Summary("releases", "Webhook URL for releases")] string? releasesUrl = null,
            [Summary("role", "Role to ping for releases")] SocketRole? role = null
        )
        {
            var interaction = Context.Interaction;
            var lng = interaction.UserLocale;
            var guildId = interaction.GuildId ?? 0;

            // Sanitize inputs
            var originGuildIdStr = serverId.Trim();
            alias = alias.Trim();
            var roleId = role?.Id;

            // Check for guild administrator status
            var guild = Nino.Client.GetGuild(guildId);
            var member = guild.GetUser(interaction.User.Id);
            if (!Utils.VerifyAdministrator(member, guild)) return await Response.Fail(T("error.notPrivileged", lng), interaction);

            // Validate no-op condition
            if (!blame && updatesUrl == null && releasesUrl == null)
                return await Response.Fail(T("error.observerNoOp", lng), interaction);

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

            // Fake project existence if private
            if (project.IsPrivate && !Utils.VerifyUser(interaction.User.Id, project))
                return await Response.Fail(T("error.alias.resolutionFailed", lng, alias), interaction);

            var observer = new Records.Observer
            {
                Id = AzureHelper.CreateObserverId(),
                GuildId = guildId,
                OriginGuildId = originGuildId,
                OwnerId = interaction.User.Id,
                ProjectId = project.Id,
                Blame = blame,
                ProgressWebhook = updatesUrl,
                ReleasesWebhook = releasesUrl,
                RoleId = roleId
            };

            // Add to database
            await AzureHelper.Observers!.UpsertItemAsync(observer);

            Log.Info($"M[{interaction.User.Id} (@{interaction.User.Username})] created new observer {observer.Id} from {guildId} observing {project}");

            // Send success embed
            var embed = new EmbedBuilder()
                .WithTitle(T("title.observer", lng))
                .WithDescription(T("observer.added", lng, project.Nickname, originGuild.Name))
                .Build();
            await interaction.FollowupAsync(embed: embed);

            await Cache.RebuildObserverCache();
            return ExecutionResult.Success;
        }
    }
}
