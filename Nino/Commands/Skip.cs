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
using System.Text;
using static Localizer.Localizer;

namespace Nino.Commands
{
    public partial class Skip(InteractionHandler handler, InteractionService commands, InteractiveService interactive) : InteractionModuleBase<SocketInteractionContext>
    {
        public InteractionService Commands { get; private set; } = commands;
        private readonly InteractionHandler _handler = handler;
        private readonly InteractiveService _interactiveService = interactive;
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        [SlashCommand("skip", "Skip a position")]
        public async Task<RuntimeResult> Handle(
            [Summary("project", "Project nickname"), Autocomplete(typeof(ProjectAutocompleteHandler))] string alias,
            [Summary("episode", "Episode number"), Autocomplete(typeof(EpisodeAutocompleteHandler))] decimal episodeNumber,
            [Summary("abbreviation", "Position shorthand"), Autocomplete(typeof(AbbreviationAutocompleteHandler))] string abbreviation
        )
        {
            var interaction = Context.Interaction;
            var lng = interaction.UserLocale;
            var gLng = interaction.GuildLocale ?? "en-US";

            // Sanitize inputs
            alias = alias.Trim();
            abbreviation = abbreviation.Trim().ToUpperInvariant();
            
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

            // Verify task is incomplete
            if (episode.Tasks.First(t => t.Abbreviation == abbreviation).Done)
                return await Response.Fail(T("error.progress.taskAlreadyDone", lng, abbreviation), interaction);

            // Check if episode will be done
            var episodeDone = !episode.Tasks.Any(t => t.Abbreviation != abbreviation && !t.Done);

            // Update database
            var taskIndex = Array.IndexOf(episode.Tasks, episode.Tasks.Single(t => t.Abbreviation == abbreviation));
            await AzureHelper.Episodes!.PatchItemAsync<Episode>(episode.Id, partitionKey: AzureHelper.EpisodePartitionKey(episode), new[] {
                PatchOperation.Set($"/tasks/{taskIndex}/done", true),
                PatchOperation.Set($"/done", episodeDone),
                PatchOperation.Set($"/updated", DateTimeOffset.Now)
            });

            // Update task for embeds
            episode.Tasks.Single(t => t.Abbreviation == abbreviation).Done = true;

            var taskTitle = project.KeyStaff.Concat(episode.AdditionalStaff).First(ks => ks.Role.Abbreviation == abbreviation).Role.Name;
            var title = T("title.progress", gLng, episodeNumber);
            var status = Cache.GetConfig(project.GuildId)?.UpdateDisplay.Equals(UpdatesDisplayType.Extended) ?? false
                ? StaffList.GenerateExplainProgress(project, episode, gLng, abbreviation) // Explanitory
                : StaffList.GenerateProgress(project, episode, abbreviation); // Standard

            status = $":fast_forward: **{taskTitle}** {T("progress.skipped.appendage", lng)}\n{status}";

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

            // Prepare success embed
            var episodeDoneText = episodeDone ? $"\n{T("progress.episodeComplete", lng, episodeNumber)}" : string.Empty;
            var replyStatus = StaffList.GenerateProgress(project, episode, abbreviation);

            var replyHeader = project.IsPrivate
                ? $"🔒 {project.Title} ({project.Type.ToFriendlyString(lng)})"
                : $"{project.Title} ({project.Type.ToFriendlyString(lng)})";

            var replyBody = Cache.GetConfig(project.GuildId)?.ProgressDisplay.Equals(ProgressDisplayType.Verbose) ?? false
                ? $"{T("progress.skipped", lng, taskTitle, episodeNumber)}\n\n{replyStatus}{episodeDoneText}" // Verbose
                : $"{T("progress.skipped", lng, taskTitle, episodeNumber)}{episodeDoneText}"; // Succinct (default)

            // Send the embed
            var replyEmbed = new EmbedBuilder()
                .WithAuthor(name: replyHeader)
                .WithTitle($":fast_forward: {T("title.taskSkipped", lng)}")
                .WithDescription(replyBody)
                .WithCurrentTimestamp()
                .Build();
            await interaction.FollowupAsync(embed: replyEmbed);

            // Everybody do the Conga!
            StringBuilder congaContent = new();

            // Get all conga participants that the current task can call out
            var congaCandidates = project.CongaParticipants
                .GroupBy(p => p.Next)
                .Where(group => group.Select(p => p.Current).Contains(abbreviation))
                .ToList();

            if (congaCandidates.Count > 0)
            {
                foreach (var candidate in congaCandidates)
                {
                    bool ping = true;
                    if (candidate.Count() > 1) // More than just this task
                    {
                        // Determine if the candidate's caller(s) (not this one) are all done
                        if (candidate.Select(c => c.Current)
                            .Where(c => c != abbreviation)
                            .Where(c => !episode.Tasks.FirstOrDefault(t => t.Abbreviation == c)?.Done ?? false)
                            .Any())
                            ping = false; // Not all caller(s) are done
                    }
                    if (ping)
                    {
                        var nextTask = project.KeyStaff.FirstOrDefault(ks => ks.Role.Abbreviation == candidate.Key);
                        if (nextTask != null)
                        {
                            var staffMention = $"<@{nextTask.UserId}>";
                            var roleTitle = nextTask.Role.Name;
                            congaContent.AppendLine(T("progress.done.conga", lng, staffMention, episode.Number, roleTitle));
                        }
                    }
                }

                if (congaContent.Length > 0)
                    await interaction.Channel.SendMessageAsync(congaContent.ToString());
            }

            await Cache.RebuildCacheForProject(project.Id);
            return ExecutionResult.Success;
        }
    }
}
