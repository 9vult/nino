using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Localizer;
using Nino.Handlers;
using Nino.Records.Enums;
using Nino.Utilities;
using static Localizer.Localizer;

namespace Nino.Commands;

public partial class Release
{
        [SlashCommand("volume", "Release a volume")]
        public async Task<RuntimeResult> Volume(
            [Summary("project", "Project nickname"), Autocomplete(typeof(ProjectAutocompleteHandler))] string alias,
            [Summary("volume", "Volume number to release")] string volumeNumber,
            [Summary("url", "Release URL(s)")] string releaseUrl,
            [Summary("role", "Role to ping")] SocketRole? role = null
        )
        {
            var interaction = Context.Interaction;
            var lng = interaction.UserLocale;
            var gLng = Cache.GetConfig(interaction.GuildId ?? 0)?.Locale?.ToDiscordLocale() ?? interaction.GuildLocale ?? "en-US";

            // Sanitize inputs
            alias = alias.Trim();
            var roleId = role?.Id;

            // Verify project and user - Owner or Admin required
            var project = Utils.ResolveAlias(alias, interaction);
            if (project == null)
                return await Response.Fail(T("error.alias.resolutionFailed", lng, alias), interaction);

            if (!Utils.VerifyUser(interaction.User.Id, project))
                return await Response.Fail(T("error.permissionDenied", lng), interaction);

            if (project.IsArchived)
                return await Response.Fail(T("error.archived", lng), interaction);

            // Check progress channel permissions
            var goOn = await PermissionChecker.Precheck(_interactiveService, interaction, project, lng, true);
            // Cancel
            if (!goOn) return ExecutionResult.Success;

            var roleStr = roleId != null
                ? roleId == project.GuildId ? "@everyone " : $"<@&{roleId}> "
                : "";

            var publishTitle = T("progress.release.volume.publish", gLng, project.Title, volumeNumber);
            var publishBody = $"**{publishTitle}**\n{roleStr}{releaseUrl}";
            
            // Add prefix if needed
            if (!string.IsNullOrEmpty(Cache.GetConfig(project.GuildId)?.ReleasePrefix))
                publishBody = $"{Cache.GetConfig(project.GuildId)!.ReleasePrefix!} {publishBody}";

            // Publish to local releases channel
            try
            {
                var publishChannel = (SocketTextChannel)Nino.Client.GetChannel(project.ReleaseChannelId);
                var msg = await publishChannel.SendMessageAsync(text: publishBody);
                if (msg.Channel.GetChannelType() == ChannelType.News) // Guild announcement channel
                    await msg.CrosspostAsync(); // Publish announcement
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
                return await Response.Fail(T("error.release.failed", lng, e.Message), interaction);
            }

            // Publish to observers
            await ObserverPublisher.PublishRelease(project, publishTitle, releaseUrl);
            
            // Send success embed
            var replyHeader = project.IsPrivate
                ? $"🔒 {project.Title} ({project.Type.ToFriendlyString(lng)})"
                : $"{project.Title} ({project.Type.ToFriendlyString(lng)})";

            var replyEmbed = new EmbedBuilder()
                .WithAuthor(name: replyHeader)
                .WithTitle(T("title.released", lng))
                .WithDescription(T("progress.release.volume.reply", lng, project.Title, volumeNumber))
                .WithCurrentTimestamp()
                .Build();
            await interaction.FollowupAsync(embed: replyEmbed);

            return ExecutionResult.Success;
        }
}
