using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Fergun.Interactive;
using Nino.Handlers;
using Nino.Records.Enums;
using Nino.Utilities;
using NLog;
using System.Text;
using Localizer;
using Nino.Records;
using static Localizer.Localizer;
using Task = System.Threading.Tasks.Task;

namespace Nino.Commands;

public partial class Skip(DataContext db, InteractiveService interactive) : InteractionModuleBase<SocketInteractionContext>
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    [SlashCommand("skip", "Skip a position")]
    public async Task<RuntimeResult> Handle(
        [Summary("project", "Project nickname"), Autocomplete(typeof(ProjectAutocompleteHandler))] string alias,
        [Summary("episode", "Episode number"), Autocomplete(typeof(EpisodeAutocompleteHandler))] string episodeNumber,
        [Summary("abbreviation", "Position shorthand"), Autocomplete(typeof(AbbreviationAutocompleteHandler))] string abbreviation
    )
    {
        var interaction = Context.Interaction;
        var lng = interaction.UserLocale;
        var config = db.GetConfig(interaction.GuildId ?? 0);
        var gLng = config?.Locale?.ToDiscordLocale() ?? interaction.GuildLocale ?? "en-US";

        // Sanitize inputs
        alias = alias.Trim();
        abbreviation = abbreviation.Trim().ToUpperInvariant();
        episodeNumber = Utils.CanonicalizeEpisodeNumber(episodeNumber);
            
        // Verify project
        var project = db.ResolveAlias(alias, interaction);
        if (project is null)
            return await Response.Fail(T("error.alias.resolutionFailed", lng, alias), interaction);

        if (project.IsArchived)
            return await Response.Fail(T("error.archived", lng), interaction);

        // Check progress channel permissions
        var goOn = await PermissionChecker.Precheck(interactive, interaction, project, lng, false);
        // Cancel
        if (!goOn) return ExecutionResult.Success;
            
        // Check Conga permissions
        if (project.CongaParticipants.Nodes.Count != 0)
        {
            goOn = await PermissionChecker.Precheck(interactive, interaction, project, lng, false, true);
            // Cancel
            if (!goOn) return ExecutionResult.Success;
        }

        // Verify episode and task
        if (!project.TryGetEpisode(episodeNumber, out var episode))
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

        task.Done = true;
        task.Updated = DateTimeOffset.UtcNow;
        episode.Done = episodeDone;
        episode.Updated = DateTimeOffset.UtcNow;

        var taskTitle = staff.Role.Name;
        var title = T("title.progress", gLng, episodeNumber);
        var status = config?.UpdateDisplay.Equals(UpdatesDisplayType.Extended) ?? false
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

        var replyBody = config?.ProgressDisplay.Equals(ProgressDisplayType.Verbose) ?? false
            ? $"{T("progress.skipped", lng, taskTitle, episodeNumber)}\n\n{replyStatus}{episodeDoneText}" // Verbose
            : $"{T("progress.skipped", lng, taskTitle, episodeNumber)}{episodeDoneText}"; // Succinct (default)

        // Send the embed
        var replyEmbed = new EmbedBuilder()
            .WithAuthor(name: replyHeader, url: project.AniListUrl)
            .WithTitle($":fast_forward: {T("title.taskSkipped", lng)}")
            .WithDescription(replyBody)
            .WithCurrentTimestamp()
            .Build();
        await interaction.FollowupAsync(embed: replyEmbed);
            
        Log.Info($"M[{interaction.User.Id} (@{interaction.User.Username})] skipped task {abbreviation} for {episode}");

        // Everybody do the Conga!
        await DoTheConga();

        await db.SaveChangesAsync();
        return ExecutionResult.Success; 
            
        // -----
            
        // Helper method for doing the conga
        async Task DoTheConga()
        {
            // Get all conga participants that the current task can call out
            var congaCandidates = project.CongaParticipants.Get(abbreviation)?.Dependents?
                .Where(dep => episode.Tasks.Any(t => t.Abbreviation == dep.Abbreviation)).ToList() ?? []; // Limit to tasks in the episode
            if (congaCandidates.Count == 0) return;
                
            var prefixMode = config?.CongaPrefix ?? CongaPrefixType.None;
            StringBuilder congaContent = new();

            var singleCandidates = new List<CongaNode>();
            var multiCandidates = new List<CongaNode>();
                
            foreach (var candidate in congaCandidates)
            {
                var prereqs = candidate.Prerequisites
                    .Where(dep => episode.Tasks.Any(t => t.Abbreviation == dep.Abbreviation)).ToList();
                if (prereqs.Count == 1)
                {
                    singleCandidates.Add(candidate);
                    continue;
                }
                // More than just this task
                // Determine if the candidate's caller(s) (not this one) are all done
                if (prereqs
                    .Select(c => c.Abbreviation)
                    .Where(c => c != abbreviation)
                    .Any(c => !episode.Tasks.FirstOrDefault(t => t.Abbreviation == c)?.Done ?? false))
                    continue; // Not all callers are done
                multiCandidates.Add(candidate); // All callers are done
            }
                
            // Because we're skipping, find out if single-prereq tasks should still be pinged
            if (singleCandidates.Count > 0)
            {
                var (pingOn, finalBody, questionMessage) 
                    = await Ask.AboutAction(interactive, interaction, project, lng,  Ask.InconsequentialAction.PingCongaAfterSkip);
                    
                if (!pingOn)
                    singleCandidates.Clear(); // Do not ping the single-prereq tasks

                if (questionMessage is not null) await questionMessage.DeleteAsync(); // Remove the question embed
            }
                
            // Ping everyone to be pung
            foreach (var candidate in multiCandidates.Concat(singleCandidates))
            {
                var nextTask = project.KeyStaff.Concat(episode.AdditionalStaff)
                    .FirstOrDefault(ks => ks.Role.Abbreviation == candidate.Abbreviation);
                if (nextTask is null) continue;
                    
                // Skip task if already done
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
                var congaTask = episode.Tasks.FirstOrDefault(t => t.Abbreviation == nextTask.Role.Abbreviation);
                if (congaTask is not null)
                    congaTask.LastReminded = DateTimeOffset.UtcNow;
            }
                
            if (congaContent.Length > 0)
                await interaction.Channel.SendMessageAsync(congaContent.ToString());
        }

        // Helper method to publish embeds to the local progress channel and to observers
        async Task PublishEmbeds()
        {
            status = $":fast_forward: **{taskTitle}** {T("progress.skipped.appendage", lng)}\n{status}";

            var publishEmbed = new EmbedBuilder()
                .WithAuthor($"{project.Title} ({project.Type.ToFriendlyString(gLng)})", url: project.AniListUrl)
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
            await ObserverPublisher.PublishProgress(project, publishEmbed, db);
        }
    }
}