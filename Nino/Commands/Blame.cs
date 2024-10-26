using System.Text;
using Discord;
using Discord.Interactions;
using Nino.Handlers;
using Nino.Records.Enums;
using Nino.Services;
using Nino.Utilities;
using NLog;
using static Localizer.Localizer;

namespace Nino.Commands
{
    public class Blame(InteractionHandler handler, InteractionService commands) : InteractionModuleBase<SocketInteractionContext>
    {
        public InteractionService Commands { get; private set; } = commands;
        private readonly InteractionHandler _handler = handler;
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        [SlashCommand("blame", "Check the status of a project")]
        public async Task<RuntimeResult> Handle(
            [Summary("project", "Project nickname"), Autocomplete(typeof(ProjectAutocompleteHandler))] string alias,
            [Summary("episode", "Episode number"), Autocomplete(typeof(EpisodeAutocompleteHandler))] decimal? episodeNumber = null,
            [Summary("explain", "Explain what any of this means")] bool explain = false
        )
        {
            var interaction = Context.Interaction;
            var lng = interaction.UserLocale;
            alias = alias.Trim();
            
            // Verify project
            var project = Utils.ResolveAlias(alias, interaction, includeObservers: true);
            if (project == null)
                return await Response.Fail(T("error.alias.resolutionFailed", lng, alias), interaction);

            var episodes = Cache.GetEpisodes(project.Id).OrderBy(e => e.Number);

            // Verify or find episode
            if (episodeNumber == null)
            {
                var nextNumber = episodes.FirstOrDefault(e => !e.Done)?.Number ?? episodes.LastOrDefault()?.Number;
                if (nextNumber == null)
                    return await Response.Fail(T("error.noEpisodes", lng), interaction);
                episodeNumber = (decimal)nextNumber;
            }
            
            var episode = await Getters.GetEpisode(project, (decimal)episodeNumber);
            if (episode == null)
                return await Response.Fail(T("error.noSuchEpisode", lng, episodeNumber), interaction);

            StringBuilder progress = new();

            // Add the project's MOTD or archival notice, if applicable
            if (!string.IsNullOrEmpty(project.Motd))
                progress.AppendLine(project.Motd);
            if (project.IsArchived)
                progress.AppendLine(T("blame.archived", lng));

            progress.AppendLine(explain ? StaffList.GenerateExplainProgress(project, episode, lng)
                : StaffList.GenerateProgress(project, episode));

            // Add any update information
            if (project.AniListId != null && !episode.Tasks.Any(t => t.Done))
            {
                var airStatus = await AirDateService.GetAirDateString((int)project.AniListId, episode.Number, lng);
                progress.AppendLine();
                progress.AppendLine(airStatus);
            }
            else if (episode.Updated != null)
            {
                progress.AppendLine();
                progress.AppendLine(T("episode.lastUpdated", lng, $"<t:{episode.Updated?.ToUnixTimeSeconds()}:R>"));
            }

            var title = project.IsPrivate
                ? $"🔒 {project.Title} ({project.Type.ToFriendlyString(lng)})"
                : $"{project.Title} ({project.Type.ToFriendlyString(lng)})";

            var embed = new EmbedBuilder()
                .WithAuthor(title)
                .WithTitle(T("title.progress", lng, episode.Number))
                .WithThumbnailUrl(project.PosterUri)
                .WithDescription(progress.ToString())
                .WithCurrentTimestamp()
                .Build();

            await interaction.FollowupAsync(embed: embed);

            return ExecutionResult.Success;
        }
    }
}
