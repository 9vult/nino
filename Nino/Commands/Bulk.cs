using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Fergun.Interactive;
using Microsoft.Azure.Cosmos;
using NaturalSort.Extension;
using Nino.Handlers;
using Nino.Records;
using Nino.Records.Enums;
using Nino.Utilities;
using NLog;
using System.Text;
using Localizer;
using static Localizer.Localizer;

namespace Nino.Commands;

public partial class Bulk(InteractiveService interactive) : InteractionModuleBase<SocketInteractionContext>
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    [SlashCommand("bulk", "Do a lot of episodes all at once!")]
    public async Task<RuntimeResult> Handle(
        [Summary("project", "Project nickname"), Autocomplete(typeof(ProjectAutocompleteHandler))] string alias,
        [Summary("action", "Action to perform")] ProgressType action,
        [Summary("abbreviation", "Position shorthand")] string abbreviation,
        [Summary("start", "Start episode number"), Autocomplete(typeof(EpisodeAutocompleteHandler))] string startEpisodeNumber,
        [Summary("end", "End episode number"), Autocomplete(typeof(EpisodeAutocompleteHandler))] string endEpisodeNumber
    )
    {
        var interaction = Context.Interaction;
        var lng = interaction.UserLocale;
        var gLng = Cache.GetConfig(interaction.GuildId ?? 0)?.Locale?.ToDiscordLocale() ?? interaction.GuildLocale ?? "en-US";

        // Sanitize inputs
        alias = alias.Trim();
        abbreviation = abbreviation.Trim().ToUpperInvariant();
        startEpisodeNumber = Utils.CanonicalizeEpisodeNumber(startEpisodeNumber);
        endEpisodeNumber = Utils.CanonicalizeEpisodeNumber(endEpisodeNumber);

        var naturalSorter = StringComparison.OrdinalIgnoreCase.WithNaturalSort();
        if (naturalSorter.Compare(endEpisodeNumber, startEpisodeNumber) <= 0)
            return await Response.Fail(T("error.invalidTimeRange", lng), interaction);
            
        // Verify project
        var project = Utils.ResolveAlias(alias, interaction);
        if (project == null)
            return await Response.Fail(T("error.alias.resolutionFailed", lng, alias), interaction);

        if (project.IsArchived)
            return await Response.Fail(T("error.archived", lng), interaction);

        // Check progress channel permissions
        var goOn = await PermissionChecker.Precheck(interactive, interaction, project, lng, false);
        // Cancel
        if (!goOn) return ExecutionResult.Success;

        // Verify episode and task
        if (!Getters.TryGetEpisode(project, startEpisodeNumber, out var startEpisode))
            return await Response.Fail(T("error.noSuchEpisode", lng, startEpisodeNumber), interaction);
        if (startEpisode.Tasks.All(t => t.Abbreviation != abbreviation))
            return await Response.Fail(T("error.noSuchTask", lng, abbreviation), interaction);

        // Verify user
        if (!Utils.VerifyTaskUser(interaction.User.Id, project, startEpisode, abbreviation))
            return await Response.Fail(T("error.permissionDenied", lng), interaction);

        var prefixMode = Cache.GetConfig(project.GuildId)?.CongaPrefix ?? CongaPrefixType.None;
        StringBuilder congaContent = new();
            
        // Update database
        List<string> completedEpisodes = [];
        var episodesToProcess = Cache.GetEpisodes(project.Id)
            .Where(e => naturalSorter.Compare(e.Number, startEpisodeNumber) >= 0
                        && naturalSorter.Compare(e.Number, endEpisodeNumber) <= 0);
        
        foreach (var chunk in episodesToProcess.Chunk(50))
        {
            var batch = AzureHelper.Episodes!.CreateTransactionalBatch(partitionKey: AzureHelper.EpisodePartitionKey(project));
            foreach (var e in chunk)
            {
                var taskIndex = Array.IndexOf(e.Tasks, e.Tasks.Single(t => t.Abbreviation == abbreviation));
                var isDone = action is ProgressType.Done or ProgressType.Skipped;

                // Check if episode will be done
                var episodeDone = isDone && !e.Tasks.Any(t => t.Abbreviation != abbreviation && !t.Done);
                if (episodeDone) completedEpisodes.Add(e.Number);

                if (taskIndex == -1) continue;
                
                var processedConga = isDone 
                    ? ProcessCongaForEpisode(project, e, abbreviation, lng, prefixMode) 
                    : (string.Empty, []);
                if (processedConga.Item1.Length > 0) congaContent.AppendLine(processedConga.Item1.Trim());
                    
                batch.PatchItem(id: e.Id.ToString(), [
                    PatchOperation.Set($"/tasks/{taskIndex}/done", isDone),
                    PatchOperation.Set($"/done", episodeDone),
                    PatchOperation.Set($"/updated", DateTimeOffset.UtcNow),
                    ..processedConga.Item2
                ]);
            }
            await batch.ExecuteAsync();
        }
        

        var staff = project.KeyStaff.Concat(startEpisode.AdditionalStaff).First(ks => ks.Role.Abbreviation == abbreviation);
        
        var taskTitle = staff.Role.Name;
        var title = T("title.progress.bulk", gLng, startEpisodeNumber, endEpisodeNumber);
            
        if (congaContent.Length > 0)
            await interaction.Channel.SendMessageAsync(congaContent.ToString());

        // Skip published embeds for pseudo-tasks
        if (!staff.IsPseudo)
        {
            var status = action switch
            {
                ProgressType.Done => $"✅ **{taskTitle}**",
                ProgressType.Undone => $"❌ **{taskTitle}**",
                ProgressType.Skipped => $":fast_forward: **{taskTitle}** {T("progress.skipped.appendage", gLng)}",
                _ => ""
            };
            
            var publishEmbed = new EmbedBuilder()
                .WithAuthor($"{project.Title} ({project.Type.ToFriendlyString(lng)})", url: project.AniListUrl)
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
                return await Response.Fail(T("error.release.failed", lng, e.Message), interaction);
            }

            // Publish to observers
            await ObserverPublisher.PublishProgress(project, publishEmbed);
        }

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
            .WithAuthor(name: replyHeader, url: project.AniListUrl)
            .WithTitle(replyTitle)
            .WithDescription(replyBody)
            .WithCurrentTimestamp()
            .Build();
        await interaction.FollowupAsync(embed: replyEmbed);
            
        Log.Info($"M[{interaction.User.Id} (@{interaction.User.Username})] batched {abbreviation} in {project} for {startEpisodeNumber}-{endEpisodeNumber}");

        await Cache.RebuildCacheForProject(project.Id);
        return ExecutionResult.Success;
    }

    /// <summary>
    /// Process an episode's conga line
    /// </summary>
    /// <param name="project">Project being processed</param>
    /// <param name="episode">Episode being processed</param>
    /// <param name="abbreviation">Task that was completed</param>
    /// <param name="lng">Language to return results in</param>
    /// <param name="prefixMode">Conga prefix type</param>
    /// <returns>Tuple of StringBuilder and PatchOperation list</returns>
    private static (string, List<PatchOperation>) ProcessCongaForEpisode (Project project, Episode episode, string abbreviation, string lng, CongaPrefixType prefixMode)
    {
        StringBuilder congaContent = new();
        List<PatchOperation> operations = [];
            
        // Get all conga participants that the current task can call out
        var congaCandidates = project.CongaParticipants.Get(abbreviation)?.Dependents?
            .Where(dep => episode.Tasks.Any(t => t.Abbreviation == dep.Abbreviation)).ToList() ?? []; // Limit to tasks in the episode
            
        if (congaCandidates.Count == 0) return (string.Empty, operations); // Empty
            
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
                        
            var congaTaskIndex = Array.IndexOf(episode.Tasks, episode.Tasks.Single(t => t.Abbreviation == nextTask.Role.Abbreviation));
            operations.Add(PatchOperation.Set($"/tasks/{congaTaskIndex}/lastReminded", DateTimeOffset.UtcNow));
        }
            
        return (congaContent.ToString(), operations);
    }
}