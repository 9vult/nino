using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Azure.Cosmos;
using Nino.Records;
using Nino.Records.Enums;
using Nino.Utilities;
using static Localizer.Localizer;

namespace Nino.Commands
{
    public partial class Done
    {
        public static async Task<RuntimeResult> HandleSpecified(SocketInteraction interaction, Project project, string abbreviation, decimal episodeNumber)
        {
            var lng = interaction.UserLocale;
            var gLng = interaction.GuildLocale ?? "en-US";

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
                PatchOperation.Set($"/done", episodeDone)
            });

            // Update task for embeds
            episode.Tasks.Single(t => t.Abbreviation == abbreviation).Done = true;

            var taskTitle = project.KeyStaff.Concat(episode.AdditionalStaff).First(ks => ks.Role.Abbreviation == abbreviation).Role.Name;
            var title = T("title.progress", gLng, episodeNumber);
            var status = Cache.GetConfig(project.GuildId)?.UpdateDisplay.Equals(UpdatesDisplayType.Extended) ?? false
                ? StaffList.GenerateExplainProgress(project, episode, gLng, abbreviation) // Explanitory
                : StaffList.GenerateProgress(project, episode, abbreviation); // Standard

            status = $"✅ **{taskTitle}**\n{status}";

            var publishEmbed = new EmbedBuilder()
                .WithAuthor(name: $"{project.Title} ({project.Type.ToFriendlyString(gLng)})")
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
            var episodeDoneText = episodeDone ? $"\n{T("progress.episodeComplete", lng, episodeNumber)}" : string.Empty;
            var replyStatus = StaffList.GenerateProgress(project, episode, abbreviation);

            var replyHeader = project.IsPrivate
                ? $"🔒 {project.Title} ({project.Type.ToFriendlyString(lng)})"
                : $"{project.Title} ({project.Type.ToFriendlyString(lng)})";

            var replyBody = Cache.GetConfig(project.GuildId)?.ProgressDisplay.Equals(ProgressDisplayType.Verbose) ?? false
                ? $"{T("progress.done", lng, taskTitle, episodeNumber)}\n\n{replyStatus}{episodeDoneText}" // Verbose
                : $"{T("progress.done", lng, taskTitle, episodeNumber)}{episodeDoneText}"; // Succinct (default)

            var replyEmbed = new EmbedBuilder()
                .WithAuthor(replyHeader)
                .WithTitle($"✅ {T("title.taskComplete", lng)}")
                .WithDescription(replyBody)
                .WithCurrentTimestamp()
                .Build();
            await interaction.FollowupAsync(embed: replyEmbed);

            // Everybody do the Conga!
            var congaEntry = project.CongaParticipants.FirstOrDefault(c => c.Current == abbreviation);
            if (congaEntry != null)
            {
                var nextTask = project.KeyStaff.FirstOrDefault(ks => ks.Role.Abbreviation == congaEntry.Next);
                if (nextTask != null)
                {
                    var staffMention = $"<@{nextTask.UserId}>";
                    var roleTitle = nextTask.Role.Name;
                    var congaContent = T("progress.done.conga", lng, staffMention, episode.Number, roleTitle);

                    await interaction.FollowupAsync(text: congaContent);
                }
            }

            await Cache.RebuildCacheForProject(project.Id);
            return ExecutionResult.Success;
        }
    }
}
