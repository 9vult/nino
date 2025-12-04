using System.Text;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Localizer;
using Nino.Records;
using Nino.Records.Enums;
using Nino.Services;
using Nino.Utilities;
using Nino.Utilities.Extensions;
using static Localizer.Localizer;
using Task = System.Threading.Tasks.Task;

namespace Nino.Commands;

public partial class Done
{
    private async Task<RuntimeResult> HandleSpecified(
        SocketInteraction interaction,
        Project project,
        string abbreviation,
        string episodeNumber
    )
    {
        Log.Info(
            $"Handling specified /done by M[{interaction.User.Id} (@{interaction.User.Username})] for {project} episode {episodeNumber}"
        );

        var config = db.GetConfig(interaction.GuildId ?? 0);
        var lng = interaction.UserLocale;
        var gLng = config?.Locale?.ToDiscordLocale() ?? interaction.GuildLocale ?? "en-US";

        // Verify episode and task
        if (!project.TryGetEpisode(episodeNumber, out var episode))
            return await Response.Fail(T("error.noSuchEpisode", lng, episodeNumber), interaction);
        if (episode.Tasks.All(t => t.Abbreviation != abbreviation))
            return await Response.Fail(T("error.noSuchTask", lng, abbreviation), interaction);

        // Verify user
        if (!episode.VerifyTaskUser(db, interaction.User.Id, abbreviation))
            return await Response.Fail(T("error.permissionDenied", lng), interaction);

        // Verify task is incomplete
        if (episode.Tasks.First(t => t.Abbreviation == abbreviation).Done)
            return await Response.Fail(
                T("error.progress.taskAlreadyDone", lng, abbreviation),
                interaction
            );

        // Check if the episode has aired
        if (
            project.AniListId is not null
            && project.AniListId > 0
            && Episode.EpisodeNumberIsInteger(episodeNumber, out var epNum)
            && await AirDateService.EpisodeAired(project.AniListId.Value, epNum) == false
        )
        {
            var (goOn, finalBody, questionMessage) = await Ask.AboutAction(
                interactive,
                interaction,
                project,
                lng,
                Ask.InconsequentialAction.MarkTaskDoneForUnairedEpisode,
                arg: episodeNumber
            );

            // Update the question embed to reflect the choice
            if (questionMessage is not null)
            {
                var header = project.IsPrivate
                    ? $"🔒 {project.Title} ({project.Type.ToFriendlyString(lng)})"
                    : $"{project.Title} ({project.Type.ToFriendlyString(lng)})";
                var editedEmbed = new EmbedBuilder()
                    .WithAuthor(header)
                    .WithTitle($"❓ {T("progress.done.inTheDust.question", lng)}")
                    .WithDescription(finalBody)
                    .WithCurrentTimestamp()
                    .Build();
                await questionMessage.ModifyAsync(m =>
                {
                    m.Components = null;
                    m.Embed = editedEmbed;
                });
            }

            if (!goOn)
                return ExecutionResult.Success;
        }

        // Check if episode will be done
        var episodeDone = !episode.Tasks.Any(t => t.Abbreviation != abbreviation && !t.Done);

        var task = episode.Tasks.Single(t => t.Abbreviation == abbreviation);
        var staff = project
            .KeyStaff.Concat(episode.AdditionalStaff)
            .First(ks => ks.Role.Abbreviation == abbreviation);

        // Update database
        task.Done = true;
        task.Updated = DateTimeOffset.UtcNow;
        episode.Done = episodeDone;
        episode.Updated = DateTimeOffset.UtcNow;

        var taskTitle = staff.Role.Name;
        var title = T("title.progress", gLng, episodeNumber);
        var status =
            config?.UpdateDisplay.Equals(UpdatesDisplayType.Extended) ?? false
                ? episode.GenerateExplainProgress(gLng, abbreviation) // Explanatory
                : episode.GenerateProgress(abbreviation); // Standard

        // Skip published embeds for pseudo-tasks
        if (!staff.IsPseudo)
            await PublishEmbeds();

        // Prepare success embed
        var episodeDoneText = episodeDone
            ? $"\n{T("progress.episodeComplete", lng, episodeNumber)}"
            : string.Empty;
        var replyStatus = episode.GenerateProgress(abbreviation, excludePseudo: false);

        var replyHeader = project.IsPrivate
            ? $"🔒 {project.Title} ({project.Type.ToFriendlyString(lng)})"
            : $"{project.Title} ({project.Type.ToFriendlyString(lng)})";

        var replyBody =
            config?.ProgressDisplay.Equals(ProgressDisplayType.Verbose) ?? false
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

        Log.Info(
            $"M[{interaction.User.Id} (@{interaction.User.Username})] marked task {abbreviation} done for {episode}"
        );

        // Everybody do the Conga!
        await DoTheConga();

        await db.TrySaveChangesAsync(interaction);
        return ExecutionResult.Success;

        // -----

        // Helper method for doing the conga
        async Task DoTheConga()
        {
            var prefixMode = config?.CongaPrefix ?? CongaPrefixType.None;
            StringBuilder congaContent = new();

            // Get all conga participants that the current task can call out
            var congaCandidates =
                project
                    .CongaParticipants.Get(abbreviation)
                    ?.Dependents.Where(dep =>
                        episode.Tasks.Any(t => t.Abbreviation == dep.Abbreviation)
                    )
                    .ToList()
                ?? []; // Limit to tasks in the episode

            if (congaCandidates.Count > 0)
            {
                foreach (var candidate in congaCandidates)
                {
                    var prereqs = candidate
                        .Prerequisites.Where(dep =>
                            episode.Tasks.Any(t => t.Abbreviation == dep.Abbreviation)
                        )
                        .ToList();
                    var ping = true;
                    if (prereqs.Count > 1) // More than just this task
                    {
                        // Determine if the candidate's caller(s) (not this one) are all done
                        if (
                            prereqs
                                .Select(c => c.Abbreviation)
                                .Where(c => c != abbreviation)
                                .Any(c =>
                                    !episode.Tasks.FirstOrDefault(t => t.Abbreviation == c)?.Done
                                    ?? false
                                )
                        )
                            ping = false; // Not all caller(s) are done
                    }

                    if (!ping)
                        continue;

                    var nextTask = project
                        .KeyStaff.Concat(episode.AdditionalStaff)
                        .FirstOrDefault(ks => ks.Role.Abbreviation == candidate.Abbreviation);
                    if (nextTask == null)
                        continue;

                    // Skip task if task is done
                    if (
                        episode
                            .Tasks.FirstOrDefault(t => t.Abbreviation == nextTask.Role.Abbreviation)
                            ?.Done
                        ?? false
                    )
                        continue;

                    var userId =
                        episode
                            .PinchHitters.FirstOrDefault(t =>
                                t.Abbreviation == nextTask.Role.Abbreviation
                            )
                            ?.UserId
                        ?? nextTask.UserId;
                    var staffMention = $"<@{userId}>";
                    var roleTitle = nextTask.Role.Name;
                    if (prefixMode != CongaPrefixType.None)
                    {
                        // Using a switch expression in the middle of string interpolation is insane btw
                        congaContent.Append(
                            $"[{prefixMode switch {
                            CongaPrefixType.Nickname => project.Nickname,
                            CongaPrefixType.Title => project.Title,
                            _ => string.Empty 
                        }}] "
                        );
                    }
                    congaContent.AppendLine(
                        T("progress.done.conga", lng, staffMention, episode.Number, roleTitle)
                    );

                    // Update database with new last-reminded time

                    var congaTask = episode.Tasks.Single(t =>
                        t.Abbreviation == nextTask.Role.Abbreviation
                    );
                    congaTask.LastReminded = DateTimeOffset.UtcNow;
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
                .WithAuthor(
                    name: $"{project.Title} ({project.Type.ToFriendlyString(gLng)})",
                    url: project.AniListUrl
                )
                .WithTitle(title)
                .WithDescription(status)
                .WithThumbnailUrl(project.PosterUri)
                .WithCurrentTimestamp()
                .Build();

            // Publish to local progress channel
            try
            {
                var publishChannel = (SocketTextChannel)
                    Nino.Client.GetChannel(project.UpdateChannelId);
                await publishChannel.SendMessageAsync(embed: publishEmbed);
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
                var guild = Nino.Client.GetGuild(interaction.GuildId ?? 0);
                await Utils.AlertError(
                    T("error.release.failed", lng, e.Message),
                    guild,
                    project.Nickname,
                    project.OwnerId,
                    "Release"
                );
            }

            // Publish to observers
            await ObserverPublisher.PublishProgress(project, publishEmbed, db);
        }
    }
}
