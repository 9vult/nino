using Discord;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using Nino.Utilities;

using static Localizer.Localizer;

namespace Nino.Commands
{
    public partial class Observer
    {
        [SlashCommand("add", "Start observing a project on another server")]
        public async Task<RuntimeResult> Add(
            [Summary("serverId", "ID of the server you want to observe")] string serverId,
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
            if (!blame && updatesUrl is null && releasesUrl is null)
                return await Response.Fail(T("error.observerNoOp", lng), interaction);
            
            // Validate invalid webhooks
            if (updatesUrl is not null)
                if (!Uri.TryCreate(updatesUrl, UriKind.Absolute, out _))
                    return await Response.Fail(T("error.observer.invalidProgressUrl", lng), interaction);
            if (releasesUrl is not null)
                if (!Uri.TryCreate(releasesUrl, UriKind.Absolute, out _))
                    return await Response.Fail(T("error.observer.invalidReleasesUrl", lng), interaction);

            // Validate observer server
            if (!ulong.TryParse(originGuildIdStr, out var originGuildId))
                return await Response.Fail(T("error.invalidServerId", lng), interaction);
            var originGuild = Nino.Client.GetGuild(originGuildId);
            if (originGuild == null)
                return await Response.Fail(T("error.noSuchServer", lng, originGuildIdStr), interaction);

            // Verify project and user access
            var project = db.ResolveAlias(alias, interaction, observingGuildId: originGuildId, includeObservers: true);
            if (project is null)
                return await Response.Fail(T("error.alias.resolutionFailed", lng, alias), interaction);

            // Fake project existence if private
            if (project.IsPrivate && !Utils.VerifyUser(interaction.User.Id, project))
                return await Response.Fail(T("error.alias.resolutionFailed", lng, alias), interaction);

            // Use existing observer ID, if it exists
            var observerId = db.Observers.Where(o => o.GuildId == guildId)
                .FirstOrDefault(o => o.OriginGuildId == originGuildId && o.ProjectId == project.Id)
                ?.Id ?? Guid.Empty;

            var observer = new Records.Observer
            {
                Id = observerId,
                GuildId = guildId,
                OriginGuildId = project.GuildId,
                ProjectId = project.Id,
                OwnerId = interaction.User.Id,
                Blame = blame,
                ProgressWebhook = updatesUrl,
                ReleasesWebhook = releasesUrl,
                RoleId = roleId
            };

            // Add to database
            await db.Observers.AddAsync(observer);

            Log.Info($"M[{interaction.User.Id} (@{interaction.User.Username})] created new observer {observer.Id} from {guildId} observing {project}");

            // If we have permission, then we want to delete the caller to avoid leaking webhook URLs
            var canDelete = PermissionChecker.CheckDeletePermissions(interaction.ChannelId ?? 0);
            RestFollowupMessage? tempReply = null;
            if (canDelete) 
                tempReply = await interaction.FollowupAsync("了解！");
            
            // Send success embed
            var embed = new EmbedBuilder()
                .WithTitle(T("title.observer", lng))
                .WithDescription(T("observer.added", lng, project.Nickname, originGuild.Name))
                .Build();
            await interaction.FollowupAsync(embed: embed);
            
            if (canDelete && tempReply is not null)
                await tempReply.DeleteAsync();

            await db.SaveChangesAsync();
            return ExecutionResult.Success;
        }
    }
}
