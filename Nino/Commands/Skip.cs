using System.Text;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Fergun.Interactive;
using Localizer;
using Nino.Handlers;
using Nino.Records;
using Nino.Records.Enums;
using Nino.Services;
using Nino.Utilities;
using Nino.Utilities.Extensions;
using NLog;
using static Localizer.Localizer;
using Task = System.Threading.Tasks.Task;

namespace Nino.Commands;

public class Skip(DataContext db, InteractiveService interactive)
    : InteractionModuleBase<SocketInteractionContext>
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    [SlashCommand("skip", "Skip a position")]
    public async Task<RuntimeResult> Handle(
        [Autocomplete(typeof(ProjectAutocompleteHandler))] string alias,
        [Autocomplete(typeof(EpisodeAutocompleteHandler))] string episodeNumber,
        [Autocomplete(typeof(AbbreviationAutocompleteHandler))] string abbreviation
    )
    {
        var interaction = Context.Interaction;
        var lng = interaction.UserLocale;
        var config = db.GetConfig(interaction.GuildId ?? 0);
        var gLng = config?.Locale?.ToDiscordLocale() ?? interaction.GuildLocale ?? "en-US";

        // Sanitize inputs
        alias = alias.Trim();
        abbreviation = abbreviation.Trim().ToUpperInvariant();
        episodeNumber = Episode.CanonicalizeEpisodeNumber(episodeNumber);

        // Verify project
        var project = await db.ResolveAlias(alias, interaction);
        if (project is null)
            return await Response.Fail(T("error.alias.resolutionFailed", lng, alias), interaction);

        if (project.IsArchived)
            return await Response.Fail(T("error.archived", lng), interaction);

        // Check progress channel permissions
        var goOn = await PermissionChecker.Precheck(interactive, interaction, project, lng);
        // Cancel
        if (!goOn)
            return ExecutionResult.Success;

        // Check Conga permissions
        if (project.CongaParticipants.Nodes.Count != 0)
        {
            goOn = await PermissionChecker.Precheck(
                interactive,
                interaction,
                project,
                lng,
                false,
                true
            );
            // Cancel
            if (!goOn)
                return ExecutionResult.Success;
        }

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
            && await AirDateService.EpisodeAired(
                project.AniListId.Value,
                epNum + (project.AniListOffset ?? 0)
            ) == false
        )
        {
            var (markDone, finalBody, questionMessage) = await Ask.AboutAction(
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

            if (!markDone)
                return ExecutionResult.Success;
        }

        // Check if episode will be done
        var episodeDone = !episode.Tasks.Any(t => t.Abbreviation != abbreviation && !t.Done);

        var task = episode.Tasks.Single(t => t.Abbreviation == abbreviation);
        var staff = project
            .KeyStaff.Concat(episode.AdditionalStaff)
            .First(ks => ks.Role.Abbreviation == abbreviation);

        task.Done = true;
        task.Updated = DateTimeOffset.UtcNow;
        episode.Done = episodeDone;
        episode.Updated = DateTimeOffset.UtcNow;

        var taskTitle = staff.Role.Name;
        var embedTitle =
            project.Type is ProjectType.Movie && project.Episodes.Count == 1
                ? null
                : T("title.progress", lng, episode.Number);
        var status =
            config?.UpdateDisplay.Equals(UpdatesDisplayType.Extended) ?? false
                ? episode.GenerateExplainProgress(gLng, abbreviation) // Explanatory
                : episode.GenerateProgress(abbreviation); // Standard

        // Skip published embeds for pseudo-tasks
        if (!(staff.IsPseudo || (project.IsPrivate && config?.PublishPrivateProgress == false)))
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

        Log.Info(
            $"M[{interaction.User.Id} (@{interaction.User.Username})] skipped task {abbreviation} for {episode}"
        );

        // Everybody do the Conga!
        await DoTheConga();

        await db.TrySaveChangesAsync(interaction);
        return ExecutionResult.Success;

        // -----

        // Helper method for doing the conga
        async Task DoTheConga()
        {
            var skippedNode = project.CongaParticipants.Get(abbreviation);
            if (skippedNode is null)
                return;
            var activatedNodes = skippedNode.GetActivatedNodes(episode);
            if (activatedNodes.Count == 0)
                return;

            var singlePrereqCandidates = activatedNodes
                .Where(n => n.Prerequisites.Count == 1)
                .ToList();
            var multiPrereqCandidates = activatedNodes
                .Where(n => n.Prerequisites.Count > 1)
                .ToList();

            // Ask the user if single-prereq tasks should be pinged
            if (singlePrereqCandidates.Count > 0)
            {
                var (pingOn, _, questionMessage) = await Ask.AboutAction(
                    interactive,
                    interaction,
                    project,
                    lng,
                    Ask.InconsequentialAction.PingCongaAfterSkip
                );

                // Remove the single-prereq nodes
                if (!pingOn)
                    activatedNodes.RemoveAll(n => singlePrereqCandidates.Contains(n));

                if (questionMessage is not null)
                    await questionMessage.DeleteAsync();
            }

            var prefixMode = config?.CongaPrefix ?? CongaPrefixType.None;
            StringBuilder congaContent = new();

            foreach (var node in activatedNodes)
            {
                var nextTask = project
                    .KeyStaff.Concat(episode.AdditionalStaff)
                    .FirstOrDefault(ks => ks.Role.Abbreviation == node.Abbreviation);
                if (nextTask is null)
                    continue;

                // Get the ID of the user to ping
                var userId =
                    episode
                        .PinchHitters.FirstOrDefault(t =>
                            t.Abbreviation == nextTask.Role.Abbreviation
                        )
                        ?.UserId
                    ?? nextTask.UserId;
                var staffMention = $"<@{userId}>";

                // Optional prefix
                if (prefixMode != CongaPrefixType.None)
                {
                    var prefix = prefixMode switch
                    {
                        CongaPrefixType.Nickname => project.Nickname,
                        CongaPrefixType.Title => project.Title,
                        _ => string.Empty,
                    };
                    congaContent.Append($"[{prefix}] ");
                }

                congaContent.AppendLine(
                    T("progress.done.conga", lng, staffMention, episode.Number, nextTask.Role.Name)
                );

                // Update the last reminded timestamp
                episode
                    .Tasks.FirstOrDefault(t => t.Abbreviation == node.Abbreviation)
                    ?.LastReminded = DateTimeOffset.UtcNow;
            }

            if (congaContent.Length > 0)
                await interaction.Channel.SendMessageAsync(congaContent.ToString());
        }

        // Helper method to publish embeds to the local progress channel and to observers
        async Task PublishEmbeds()
        {
            status =
                $":fast_forward: **{taskTitle}** {T("progress.skipped.appendage", lng)}\n{status}";

            var publishEmbed = new EmbedBuilder()
                .WithAuthor(
                    $"{project.Title} ({project.Type.ToFriendlyString(gLng)})",
                    url: project.AniListUrl
                )
                .WithTitle(embedTitle)
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
