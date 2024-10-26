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
    public partial class Bulk(InteractionHandler handler, InteractionService commands, InteractiveService interactive) : InteractionModuleBase<SocketInteractionContext>
    {
        public InteractionService Commands { get; private set; } = commands;
        private readonly InteractionHandler _handler = handler;
        private readonly InteractiveService _interactiveService = interactive;
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        [SlashCommand("bulk", "Do a lot of episodes all at once!")]
        public async Task<RuntimeResult> Handle(
            [Summary("project", "Project nickname"), Autocomplete(typeof(ProjectAutocompleteHandler))] string alias,
            [Summary("action", "Action to perform")] ProgressType action,
            [Summary("abbreviation", "Position shorthand")] string abbreviation,
            [Summary("start", "Start episode number"), Autocomplete(typeof(EpisodeAutocompleteHandler))] decimal startEpisodeNumber,
            [Summary("end", "End episode number"), Autocomplete(typeof(EpisodeAutocompleteHandler))] decimal endEpisodeNumber
        )
        {
            var interaction = Context.Interaction;
            var lng = interaction.UserLocale;
            var gLng = interaction.GuildLocale ?? "en-US";

            // Sanitize inputs
            alias = alias.Trim();
            abbreviation = abbreviation.Trim().ToUpperInvariant();

            if (endEpisodeNumber <= startEpisodeNumber)
                return await Response.Fail(T("error.invalidTimeRange", lng), interaction);
            
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
            var startEpisode = await Getters.GetEpisode(project, startEpisodeNumber);
            if (startEpisode == null)
                return await Response.Fail(T("error.noSuchEpisode", lng, startEpisodeNumber), interaction);
            if (!startEpisode.Tasks.Any(t => t.Abbreviation == abbreviation))
                return await Response.Fail(T("error.noSuchTask", lng, abbreviation), interaction);

            // Verify user
            if (!Utils.VerifyTaskUser(interaction.User.Id, project, startEpisode, abbreviation))
                return await Response.Fail(T("error.permissionDenied", lng), interaction);

            // Update database
            List<decimal> completedEpisodes = [];
            TransactionalBatch batch = AzureHelper.Episodes!.CreateTransactionalBatch(partitionKey: AzureHelper.EpisodePartitionKey(project));
            foreach (Episode e in (await Getters.GetEpisodes(project)).Where(e => e.Number >= startEpisodeNumber && e.Number <= endEpisodeNumber))
            {
                var taskIndex = Array.IndexOf(e.Tasks, e.Tasks.Single(t => t.Abbreviation == abbreviation));
                var isDone = action == ProgressType.Done || action == ProgressType.Skipped;

                // Check if episode will be done
                var episodeDone = isDone && !e.Tasks.Any(t => t.Abbreviation != abbreviation && !t.Done);
                if (episodeDone) completedEpisodes.Add(e.Number);

                if (taskIndex != -1)
                {
                    batch.PatchItem(id: e.Id, [
                        PatchOperation.Set($"/tasks/{taskIndex}/done", isDone),
                        PatchOperation.Set($"/done", episodeDone),
                        PatchOperation.Set($"/updated", DateTimeOffset.Now)
                    ]);
                }
            }
            await batch.ExecuteAsync();

            var taskTitle = project.KeyStaff.Concat(startEpisode.AdditionalStaff).First(ks => ks.Role.Abbreviation == abbreviation).Role.Name;
            var title = T("title.progress.bulk", gLng, startEpisodeNumber, endEpisodeNumber);
            var status = action switch
            {
                ProgressType.Done => $"✅ **{taskTitle}**",
                ProgressType.Undone => $"❌ **{taskTitle}**",
                ProgressType.Skipped => $":fast_forward: **{taskTitle}** {T("progress.skipped.appendage", gLng)}",
                _ => ""
            };

            var publishEmbed = new EmbedBuilder()
                .WithAuthor($"{project.Title} ({project.Type.ToFriendlyString(lng)})")
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
                return await Response.Fail(T("error.release.failed", lng, e.Message), interaction);
            }

            // Publish to observers
            await ObserverPublisher.PublishProgress(project, publishEmbed);

            // Send success embed
            var replyHeader = project.IsPrivate
                ? $"🔒 {project.Title} ({project.Type.ToFriendlyString(lng)})"
                : $"{project.Title} ({project.Type.ToFriendlyString(lng)})";

            var replyBody = T("progress.bulk", lng, taskTitle, startEpisodeNumber, endEpisodeNumber);
            if (completedEpisodes.Count > 0)
            {
                Dictionary<string, object> map = new()
                {
                    ["count"] = completedEpisodes.Count,
                    ["list"] = string.Join(", ", completedEpisodes)
                };
                replyBody = $"{replyBody}\n{T("progress.bulk.episodeComplete", lng, args: map, pluralName: "count")}";
            }

            var replyTitle = action switch
            {
                ProgressType.Done => $"✅ {T("title.taskComplete", lng)}",
                ProgressType.Undone => $"❌ {T("title.taskIncomplete", lng)}",
                ProgressType.Skipped => $":fast_forward: {T("title.taskSkipped", lng)}",
                _ => ""
            };

            var replyEmbed = new EmbedBuilder()
                .WithAuthor(name: replyHeader)
                .WithTitle(replyTitle)
                .WithDescription(replyBody)
                .WithCurrentTimestamp()
                .Build();
            await interaction.FollowupAsync(embed: replyEmbed);

            await Cache.RebuildCacheForProject(project.Id);
            return ExecutionResult.Success;
        }
    }
}
