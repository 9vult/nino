using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Fergun.Interactive;
using Microsoft.Azure.Cosmos;
using Nino.Handlers;
using Nino.Records;
using Nino.Records.Enums;
using Nino.Utilities;
using NLog;
using static Localizer.Localizer;

namespace Nino.Commands
{
    public partial class Undone(InteractionHandler handler, InteractionService commands, InteractiveService interactive) : InteractionModuleBase<SocketInteractionContext>
    {
        public InteractionService Commands { get; private set; } = commands;
        private readonly InteractionHandler _handler = handler;
        private readonly InteractiveService _interactiveService = interactive;
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        [SlashCommand("undone", "Mark a position as not done")]
        public async Task<RuntimeResult> Handle(
            [Summary("project", "Project nickname"), Autocomplete(typeof(ProjectAutocompleteHandler))] string alias,
            [Summary("episode", "Episode number"), Autocomplete(typeof(EpisodeAutocompleteHandler))] string episodeNumber,
            [Summary("abbreviation", "Position shorthand"), Autocomplete(typeof(AbbreviationAutocompleteHandler))] string abbreviation
        )
        {
            var interaction = Context.Interaction;
            var lng = interaction.UserLocale;
            var gLng = Cache.GetConfig(interaction.GuildId ?? 0)?.Locale?.ToDiscordLocale() ?? interaction.GuildLocale ?? "en-US";

            // Sanitize inputs
            alias = alias.Trim();
            abbreviation = abbreviation.Trim().ToUpperInvariant();
            episodeNumber = Utils.CanonicalizeEpisodeNumber(episodeNumber);
            
            // Verify project
            var project = Utils.ResolveAlias(alias, interaction);
            if (project == null)
                return await Response.Fail(T("error.alias.resolutionFailed", lng, alias), interaction);

            if (project.IsArchived)
                return await Response.Fail(T("error.archived", lng), interaction);

            // Check progress channel permissions
            var goOn = await PermissionChecker.Precheck(_interactiveService, interaction, project, lng, false);
            // Cancel
            if (!goOn) return ExecutionResult.Success;

            // Verify episode and task
            var episode = await Getters.GetEpisode(project, episodeNumber);
            if (episode == null)
                return await Response.Fail(T("error.noSuchEpisode", lng, episodeNumber), interaction);
            if (!episode.Tasks.Any(t => t.Abbreviation == abbreviation))
                return await Response.Fail(T("error.noSuchTask", lng, abbreviation), interaction);

            // Verify user
            if (!Utils.VerifyTaskUser(interaction.User.Id, project, episode, abbreviation))
                return await Response.Fail(T("error.permissionDenied", lng), interaction);

            // Verify task is complete
            if (!episode.Tasks.First(t => t.Abbreviation == abbreviation).Done)
                return await Response.Fail(T("error.progress.taskNotDone", lng, abbreviation), interaction);

            // Update database
            var taskIndex = Array.IndexOf(episode.Tasks, episode.Tasks.Single(t => t.Abbreviation == abbreviation));
            await AzureHelper.Episodes!.PatchItemAsync<Episode>(episode.Id, partitionKey: AzureHelper.EpisodePartitionKey(episode), new[] {
                PatchOperation.Set($"/tasks/{taskIndex}/done", false),
                PatchOperation.Set($"/done", false),
                PatchOperation.Set($"/updated", DateTimeOffset.Now)
            });

            // Update task for embeds
            episode.Tasks.Single(t => t.Abbreviation == abbreviation).Done = false;

            var taskTitle = project.KeyStaff.Concat(episode.AdditionalStaff).First(ks => ks.Role.Abbreviation == abbreviation).Role.Name;
            var title = T("title.progress", gLng, episodeNumber);
            var status = Cache.GetConfig(project.GuildId)?.UpdateDisplay.Equals(UpdatesDisplayType.Extended) ?? false
                ? StaffList.GenerateExplainProgress(project, episode, gLng, abbreviation) // Explanitory
                : StaffList.GenerateProgress(project, episode, abbreviation); // Standard

            status = $"❌ **{taskTitle}**\n{status}";

            var publishEmbed = new EmbedBuilder()
                .WithAuthor($"{project.Title} ({project.Type.ToFriendlyString(gLng)})")
                .WithTitle(title)
                .WithDescription(status)
                .WithThumbnailUrl(project.PosterUri)
                .WithCurrentTimestamp()
                .Build();

            // Publish to local progress channel
            try
            {
                var publishChannel = (SocketTextChannel)Nino.Client.GetChannel(project.UpdateChannelId);
                await publishChannel.SendMessageAsync(embed: publishEmbed);
            }
            catch (Exception e)
            {
                log.Error(e.Message);
                var guild = Nino.Client.GetGuild(interaction.GuildId ?? 0);
                await Utils.AlertError(T("error.release.failed", lng, e.Message), guild, project.Nickname, project.OwnerId, "Release");
            }

            // Publish to observers
            await ObserverPublisher.PublishProgress(project, publishEmbed);

            // Send success embed
            var replyStatus = StaffList.GenerateProgress(project, episode, abbreviation);

            var replyHeader = project.IsPrivate
                ? $"🔒 {project.Title} ({project.Type.ToFriendlyString(lng)})"
                : $"{project.Title} ({project.Type.ToFriendlyString(lng)})";

            var replyBody = Cache.GetConfig(project.GuildId)?.ProgressDisplay.Equals(ProgressDisplayType.Verbose) ?? false
                ? $"{T("progress.undone", lng, episodeNumber, taskTitle)}\n\n{replyStatus}" // Verbose
                : $"{T("progress.undone", lng, episodeNumber, taskTitle)}"; // Succinct (default)

            var replyEmbed = new EmbedBuilder()
                .WithAuthor(name: replyHeader)
                .WithTitle($"❌ {T("title.taskIncomplete", lng)}")
                .WithDescription(replyBody)
                .WithCurrentTimestamp()
                .Build();
            await interaction.FollowupAsync(embed: replyEmbed);

            await Cache.RebuildCacheForProject(project.Id);
            return ExecutionResult.Success;
        }
    }
}
