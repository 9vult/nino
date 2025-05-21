using System.Text;
using Discord;
using Discord.Interactions;
using NaturalSort.Extension;
using Nino.Handlers;
using Nino.Records.Enums;
using Nino.Services;
using Nino.Utilities;
using NLog;
using static Localizer.Localizer;

namespace Nino.Commands;

public class Blame : InteractionModuleBase<SocketInteractionContext>
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    [SlashCommand("blame", "Check the status of a project")]
    public async Task<RuntimeResult> Handle(
        [Summary("project", "Project nickname"), Autocomplete(typeof(ProjectAutocompleteHandler))] string alias,
        [Summary("episode", "Episode number"), Autocomplete(typeof(EpisodeAutocompleteHandler))] string? episodeNumber = null,
        [Summary("explain", "Explain what any of this means")] bool explain = false
    )
    {
        var inputEpisodeNumber = episodeNumber;
        var interaction = Context.Interaction;
        var lng = interaction.UserLocale;
        alias = alias.Trim();
            
        // Verify project
        var project = Utils.ResolveAlias(alias, interaction, includeObservers: true);
        if (project == null)
            return await Response.Fail(T("error.alias.resolutionFailed", lng, alias), interaction);

        var episodes = Cache.GetEpisodes(project.Id).OrderBy(e => e.Number, StringComparison.OrdinalIgnoreCase.WithNaturalSort()).ToList();

        // Verify or find episode
        if (episodeNumber == null)
        {
            var nextNumber = episodes.FirstOrDefault(e => !e.Done)?.Number ?? episodes.LastOrDefault()?.Number;
            if (nextNumber == null)
                return await Response.Fail(T("error.noEpisodes", lng), interaction);
            episodeNumber = nextNumber;
        }
        else
        {
            episodeNumber = Utils.CanonicalizeEpisodeNumber(episodeNumber);
        }
            
        if (!Getters.TryGetEpisode(project, episodeNumber, out var episode))
            return await Response.Fail(T("error.noSuchEpisode", lng, episodeNumber), interaction);
            
        Log.Trace($"Blaming {project} episode {episode} for M[{interaction.User.Id} (@{interaction.User.Username})]");

        var title = project.IsPrivate
            ? $"🔒 {project.Title} ({project.Type.ToFriendlyString(lng)})"
            : $"{project.Title} ({project.Type.ToFriendlyString(lng)})";

        // If no episode was specified, and the resulting episode
        // is complete, then the project is complete...
        // So send a "project complete" embed!
        if (inputEpisodeNumber == null && episode.Tasks.All(t => t.Done))
        {
            var doneEmbed = new EmbedBuilder()
                .WithAuthor(title, url: project.AniListUrl)
                .WithTitle(T("title.blame", lng))
                .WithThumbnailUrl(project.PosterUri)
                .WithDescription(T("blame.projectComplete", lng))
                .WithCurrentTimestamp()
                .Build();
            await interaction.FollowupAsync(embed: doneEmbed);
            return ExecutionResult.Success;
        }

        // Generate a blame embed
        StringBuilder progress = new();

        // Add the project's MOTD or archival notice, if applicable
        if (!string.IsNullOrEmpty(project.Motd))
            progress.AppendLine(project.Motd);
        if (project.IsArchived)
            progress.AppendLine(T("blame.archived", lng));

        progress.AppendLine(explain ? StaffList.GenerateExplainProgress(project, episode, lng)
            : StaffList.GenerateProgress(project, episode));

        // Add any update information
        if (project.AniListId != null && !episode.Tasks.Any(t => t.Done) && Utils.EpisodeNumberIsNumber(episode.Number, out var decimalNumber))
        {
            var airStatus = await AirDateService.GetAirDateString((int)project.AniListId, decimalNumber + (project.AniListOffset ?? 0), lng);
            progress.AppendLine();
            progress.AppendLine(airStatus);
        }
        else if (episode.Updated != null)
        {
            progress.AppendLine();
            progress.AppendLine(T("episode.lastUpdated", lng, $"<t:{episode.Updated?.ToUnixTimeSeconds()}:R>"));
        }

        var resultEmbed = new EmbedBuilder()
            .WithAuthor(title, url: project.AniListUrl)
            .WithTitle(T("title.progress", lng, episode.Number))
            .WithThumbnailUrl(project.PosterUri)
            .WithDescription(progress.ToString())
            .WithCurrentTimestamp()
            .Build();

        await interaction.FollowupAsync(embed: resultEmbed);

        return ExecutionResult.Success;
    }
}