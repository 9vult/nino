using System.Text;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using Nino.Records;
using Nino.Records.Enums;
using Nino.Utilities;
using static Localizer.Localizer;

namespace Nino.Commands
{
    internal static partial class Done
    {
        public static async Task<bool> HandleSpecified(SocketSlashCommand interaction, Project project, string abbreviation)
        {
            var lng = interaction.UserLocale;
            var episodeNumber = Convert.ToDecimal(interaction.Data.Options.FirstOrDefault(o => o.Name == "episode")!.Value);

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
            var title = $"✅ {taskTitle}";
            var status = Cache.GetConfig(project.GuildId)?.UpdateDisplay.Equals(DisplayType.Extended) ?? false
                ? StaffList.GenerateExplainProgress(project, episode, lng, abbreviation) // Explanitory
                : StaffList.GenerateProgress(project, episode, abbreviation); // Standard

            var publishEmbed = new EmbedBuilder()
                .WithAuthor($"{project.Title} ({project.Type.ToFriendlyString()})")
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

            var replyBody = Cache.GetConfig(project.GuildId)?.UpdateDisplay.Equals(DisplayType.Verbose) ?? false
                ? $"{T("progress.done", lng, taskTitle, episodeNumber)}\n\n{replyStatus}{episodeDoneText}" // Verbose
                : $"{T("progress.done", lng, taskTitle, episodeNumber)}{episodeDoneText}"; // Succinct (default)

            var replyEmbed = new EmbedBuilder()
                .WithAuthor(name: $"{project.Title} ({project.Type.ToFriendlyString()})")
                .WithTitle($"✅ {T("title.taskComplete", lng)}")
                .WithDescription(replyBody)
                .WithCurrentTimestamp()
                .Build();
            await interaction.FollowupAsync(embed: replyEmbed);

            // TODO: Conga

            await Cache.RebuildCacheForProject(project.Id);
            return true;
        }
    }
}
