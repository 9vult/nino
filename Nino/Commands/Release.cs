using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Nino.Handlers;
using Nino.Records.Enums;
using Nino.Utilities;
using NLog;
using static Localizer.Localizer;

namespace Nino.Commands
{
    public class Release(InteractionHandler handler, InteractionService commands) : InteractionModuleBase<SocketInteractionContext>
    {
        public InteractionService Commands { get; private set; } = commands;
        private readonly InteractionHandler _handler = handler;
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        [SlashCommand("release", "Release!")]
        public async Task<RuntimeResult> Handle(
            [Summary("project", "Project nickname")] string alias,
            [Summary("type", "Type of release")] ReleaseType releaseType,
            [Summary("number", "What is being released?")] string releaseNumber,
            [Summary("url", "Release URL(s)")] string releaseUrl,
            [Summary("role", "Role to ping")] SocketRole? role = null
        )
        {
            var interaction = Context.Interaction;
            var lng = interaction.UserLocale;

            // Sanitize inputs
            alias = alias.Trim();
            var roleId = role?.Id;

            // Verify project and user - Owner or Admin required
            var project = Utils.ResolveAlias(alias, interaction);
            if (project == null)
                return await Response.Fail(T("error.alias.resolutionFailed", lng, alias), interaction);

            if (!Utils.VerifyUser(interaction.User.Id, project))
                return await Response.Fail(T("error.permissionDenied", lng), interaction);

            var roleStr = roleId != null
                ? roleId == project.GuildId ? "@everyone " : $"<@&{roleId}> "
                : "";

            var publishBody = releaseType != ReleaseType.Custom
                ? $"**{project.Title} - {releaseType.ToFriendlyString()} {releaseNumber}**\n{roleStr}{releaseUrl}"
                : $"**{project.Title} - {releaseNumber}**\n{roleStr}{releaseUrl}";
            
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
                log.Error(e.Message);
                return await Response.Fail(T("error.release.failed", lng, e.Message), interaction);
            }

            // Publish to observers
            await ObserverPublisher.PublishRelease(project, releaseType, releaseNumber, releaseUrl);
            
            // Send success embed
            var replyEmbed = new EmbedBuilder()
                .WithAuthor(name: $"{project.Title} ({project.Type.ToFriendlyString()})")
                .WithTitle(T("title.released", lng))
                .WithDescription(T("progress.released", lng, project.Title, releaseType.ToFriendlyString(), releaseNumber))
                .WithCurrentTimestamp()
                .Build();
            await interaction.FollowupAsync(embed: replyEmbed);

            return ExecutionResult.Success;
        }
    }
}
