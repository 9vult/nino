using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Fergun.Interactive;
using Localizer;
using Nino.Handlers;
using Nino.Records;
using Nino.Records.Enums;
using Nino.Utilities;
using Nino.Utilities.Extensions;
using NLog;
using static Localizer.Localizer;
using Task = System.Threading.Tasks.Task;

namespace Nino.Commands;

public class Undone(DataContext db, InteractiveService interactive)
    : InteractionModuleBase<SocketInteractionContext>
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    [SlashCommand("undone", "Mark a position as not done")]
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

        // Verify episode and task
        if (!project.TryGetEpisode(episodeNumber, out var episode))
            return await Response.Fail(T("error.noSuchEpisode", lng, episodeNumber), interaction);
        if (episode.Tasks.All(t => t.Abbreviation != abbreviation))
            return await Response.Fail(T("error.noSuchTask", lng, abbreviation), interaction);

        // Verify user
        if (!episode.VerifyTaskUser(db, interaction.User.Id, abbreviation))
            return await Response.Fail(T("error.permissionDenied", lng), interaction);

        // Verify task is complete
        if (!episode.Tasks.First(t => t.Abbreviation == abbreviation).Done)
            return await Response.Fail(
                T("error.progress.taskNotDone", lng, abbreviation),
                interaction
            );

        var task = episode.Tasks.Single(t => t.Abbreviation == abbreviation);
        var staff = project
            .KeyStaff.Concat(episode.AdditionalStaff)
            .First(ks => ks.Role.Abbreviation == abbreviation);

        task.Done = false;
        task.Updated = DateTimeOffset.UtcNow;
        episode.Done = false;
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

        // Send success embed
        var replyStatus = episode.GenerateProgress(abbreviation, excludePseudo: false);

        var replyHeader = project.IsPrivate
            ? $"🔒 {project.Title} ({project.Type.ToFriendlyString(lng)})"
            : $"{project.Title} ({project.Type.ToFriendlyString(lng)})";

        var replyBody =
            config?.ProgressDisplay.Equals(ProgressDisplayType.Verbose) ?? false
                ? $"{T("progress.undone", lng, episodeNumber, taskTitle)}\n\n{replyStatus}" // Verbose
                : $"{T("progress.undone", lng, episodeNumber, taskTitle)}"; // Succinct (default)

        var replyEmbed = new EmbedBuilder()
            .WithAuthor(name: replyHeader, url: project.AniListUrl)
            .WithTitle($"❌ {T("title.taskIncomplete", lng)}")
            .WithDescription(replyBody)
            .WithCurrentTimestamp()
            .Build();
        await interaction.FollowupAsync(embed: replyEmbed);

        Log.Info(
            $"M[{interaction.User.Id} (@{interaction.User.Username})] marked task {abbreviation} undone for {episode}"
        );

        await db.TrySaveChangesAsync(interaction);
        return ExecutionResult.Success;

        // -----

        // Helper method to publish embeds to the local progress channel and to observers
        async Task PublishEmbeds()
        {
            status = $"❌ **{taskTitle}**\n{status}";

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
