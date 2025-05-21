using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Azure.Cosmos;
using Nino.Records;
using Nino.Records.Enums;
using Nino.Utilities;
using System.Text;
using Localizer;
using static Localizer.Localizer;
using Task = System.Threading.Tasks.Task;

namespace Nino.Commands;

public partial class Done
{
    public static async Task<RuntimeResult> HandleSpecified(SocketInteraction interaction, Project project, string abbreviation, string episodeNumber)
    {
        Log.Info($"Handling specified /done by M[{interaction.User.Id} (@{interaction.User.Username})] for {project} episode {episodeNumber}");
            
        var lng = interaction.UserLocale;
        var gLng = Cache.GetConfig(interaction.GuildId ?? 0)?.Locale?.ToDiscordLocale() ?? interaction.GuildLocale ?? "en-US";

        // Verify episode and task
        if (!Getters.TryGetEpisode(project, episodeNumber, out var episode))
            return await Response.Fail(T("error.noSuchEpisode", lng, episodeNumber), interaction);
        if (episode.Tasks.All(t => t.Abbreviation != abbreviation))
            return await Response.Fail(T("error.noSuchTask", lng, abbreviation), interaction);

        // Verify user
        if (!Utils.VerifyTaskUser(interaction.User.Id, project, episode, abbreviation))
            return await Response.Fail(T("error.permissionDenied", lng), interaction);

        // Verify task is incomplete
        if (episode.Tasks.First(t => t.Abbreviation == abbreviation).Done)
            return await Response.Fail(T("error.progress.taskAlreadyDone", lng, abbreviation), interaction);

        // Check if episode will be done
        var episodeDone = !episode.Tasks.Any(t => t.Abbreviation != abbreviation && !t.Done);

        var task = episode.Tasks.Single(t => t.Abbreviation == abbreviation);
        var staff = project.KeyStaff.Concat(episode.AdditionalStaff)
            .First(ks => ks.Role.Abbreviation == abbreviation);
            
        // Update database
        var taskIndex = Array.IndexOf(episode.Tasks, task);
        await AzureHelper.PatchEpisodeAsync(episode, [
            PatchOperation.Set($"/tasks/{taskIndex}/done", true),
            PatchOperation.Set($"/tasks/{taskIndex}/updated", DateTimeOffset.UtcNow),
            PatchOperation.Set($"/done", episodeDone),
            PatchOperation.Set($"/updated", DateTimeOffset.UtcNow)
        ]);
            
        // Update task for embeds
        task.Done = true;
            
        var taskTitle = staff.Role.Name;
        var title = T("title.progress", gLng, episodeNumber);
        var status = Cache.GetConfig(project.GuildId)?.UpdateDisplay.Equals(UpdatesDisplayType.Extended) ?? false
            ? StaffList.GenerateExplainProgress(project, episode, gLng, abbreviation) // Explanatory
            : StaffList.GenerateProgress(project, episode, abbreviation); // Standard

        // Skip published embeds for pseudo-tasks
        if (!staff.IsPseudo) await PublishEmbeds();

        // Prepare success embed
        var episodeDoneText = episodeDone ? $"\n{T("progress.episodeComplete", lng, episodeNumber)}" : string.Empty;
        var replyStatus = StaffList.GenerateProgress(project, episode, abbreviation, excludePseudo: false);

        var replyHeader = project.IsPrivate
            ? $"🔒 {project.Title} ({project.Type.ToFriendlyString(lng)})"
            : $"{project.Title} ({project.Type.ToFriendlyString(lng)})";

        var replyBody = Cache.GetConfig(project.GuildId)?.ProgressDisplay.Equals(ProgressDisplayType.Verbose) ?? false
            ? $"{T("progress.done", lng, taskTitle, episodeNumber)}\n\n{replyStatus}{episodeDoneText}" // Verbose
            : $"{T("progress.done", lng, taskTitle, episodeNumber)}{episodeDoneText}"; // Succinct (default)

        // Send the embed
        var replyEmbed = new EmbedBuilder()
            .WithAuthor(replyHeader, url: project.AniListUrl)
            .WithTitle($"✅ {T("title.taskComplete", lng)}")
            .WithDescription(replyBody)
            .WithCurrentTimestamp()
            .Build();
        await interaction.FollowupAsync(embed: replyEmbed);
            
        Log.Info($"M[{interaction.User.Id} (@{interaction.User.Username})] marked task {abbreviation} done for {episode}");

        // Everybody do the Conga!
        await DoTheConga();

        await Cache.RebuildCacheForProject(project.Id);
        return ExecutionResult.Success;
        
        // -----

        // Helper method for doing the conga
        async Task DoTheConga()
        {
            var prefixMode = Cache.GetConfig(project.GuildId)?.CongaPrefix ?? CongaPrefixType.None;
            StringBuilder congaContent = new();

            // Get all conga participants that the current task can call out
            var congaCandidates = project.CongaParticipants.Get(abbreviation)?.Dependents?
                .Where(dep => episode.Tasks.Any(t => t.Abbreviation == dep.Abbreviation)).ToList() ?? []; // Limit to tasks in the episode

            if (congaCandidates.Count > 0)
            {
                foreach (var candidate in congaCandidates)
                {
                    var prereqs = candidate.Prerequisites
                        .Where(dep => episode.Tasks.Any(t => t.Abbreviation == dep.Abbreviation)).ToList();
                    var ping = true;
                    if (prereqs.Count > 1) // More than just this task
                    {
                        // Determine if the candidate's caller(s) (not this one) are all done
                        if (prereqs
                            .Select(c => c.Abbreviation)
                            .Where(c => c != abbreviation)
                            .Any(c => !episode.Tasks.FirstOrDefault(t => t.Abbreviation == c)?.Done ?? false))
                            ping = false; // Not all caller(s) are done
                    }

                    if (!ping) continue;
                    
                    var nextTask = project.KeyStaff.Concat(episode.AdditionalStaff)
                        .FirstOrDefault(ks => ks.Role.Abbreviation == candidate.Abbreviation);
                    if (nextTask == null) continue;
                    
                    // Skip task if task is done
                    if (episode.Tasks.FirstOrDefault(t => t.Abbreviation == nextTask.Role.Abbreviation)?.Done ?? false) continue;

                    var userId = episode.PinchHitters.FirstOrDefault(t => t.Abbreviation == nextTask.Role.Abbreviation)?.UserId ?? nextTask.UserId;
                    var staffMention = $"<@{userId}>";
                    var roleTitle = nextTask.Role.Name;
                    if (prefixMode != CongaPrefixType.None)
                    {
                        // Using a switch expression in the middle of string interpolation is insane btw
                        congaContent.Append($"[{prefixMode switch {
                            CongaPrefixType.Nickname => project.Nickname,
                            CongaPrefixType.Title => project.Title,
                            _ => string.Empty 
                        }}] ");
                    }
                    congaContent.AppendLine(T("progress.done.conga", lng, staffMention, episode.Number, roleTitle));
                            
                    // Update database with new last-reminded time
                    var congaTaskIndex = Array.IndexOf(episode.Tasks, episode.Tasks.Single(t => t.Abbreviation == nextTask.Role.Abbreviation));
                    await AzureHelper.PatchEpisodeAsync(episode, [
                        PatchOperation.Set($"/tasks/{congaTaskIndex}/lastReminded", DateTimeOffset.UtcNow)
                    ]);
                }
                if (congaContent.Length > 0)
                    await interaction.Channel.SendMessageAsync(congaContent.ToString());
            }
        }
        
        // Helper method to publish embeds to the local progress channel and to observers
        async Task PublishEmbeds()
        {
            status = $"✅ **{taskTitle}**\n{status}";

            var publishEmbed = new EmbedBuilder()
                .WithAuthor(name: $"{project.Title} ({project.Type.ToFriendlyString(gLng)})", url: project.AniListUrl)
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
                Log.Error(e.Message);
                var guild = Nino.Client.GetGuild(interaction.GuildId ?? 0);
                await Utils.AlertError(T("error.release.failed", lng, e.Message), guild, project.Nickname, project.OwnerId, "Release");
            }

            // Publish to observers
            await ObserverPublisher.PublishProgress(project, publishEmbed);
        }
    }
}