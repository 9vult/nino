using Discord;
using Discord.Interactions;
using Nino.Handlers;
using Nino.Records;
using Nino.Records.Enums;
using Nino.Utilities;
using Nino.Utilities.Extensions;
using static Localizer.Localizer;

namespace Nino.Commands;

public partial class Episodes
{
    public class EpisodeRoster(DataContext db) : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("roster", "See who's working on an episode")]
        public async Task<RuntimeResult> Handle(
            [Autocomplete(typeof(ProjectAutocompleteHandler))] string alias,
            [Autocomplete(typeof(EpisodeAutocompleteHandler))] string episodeNumber,
            bool withWeights = false
        )
        {
            var interaction = Context.Interaction;
            var lng = interaction.UserLocale;

            // Sanitize inputs
            alias = alias.Trim();
            episodeNumber = Episode.CanonicalizeEpisodeNumber(episodeNumber);

            // Verify project and user - minimum Key Staff required
            var project = await db.ResolveAlias(alias, interaction);
            if (project is null)
                return await Response.Fail(
                    T("error.alias.resolutionFailed", lng, alias),
                    interaction
                );

            if (!project.VerifyUser(db, interaction.User.Id, includeStaff: true))
                return await Response.Fail(T("error.permissionDenied", lng), interaction);

            // Verify episode
            if (!project.TryGetEpisode(episodeNumber, out var episode))
                return await Response.Fail(
                    T("error.noSuchEpisode", lng, episodeNumber),
                    interaction
                );

            Log.Trace(
                $"Generating roster for {project} episode {episode} for M[{interaction.User.Id} (@{interaction.User.Username})]"
            );

            if (project.KeyStaff.Count == 0)
                return await Response.Fail(T("error.noRoster", lng), interaction);

            var roster = episode.GenerateRoster(withWeights, excludePseudo: false);
            var title = project.IsPrivate
                ? $"🔒 {project.Title} ({project.Type.ToFriendlyString(lng)})"
                : $"{project.Title} ({project.Type.ToFriendlyString(lng)})";

            var embedTitle =
                project.Type is ProjectType.Movie && project.Episodes.Count == 1
                    ? null
                    : T("title.progress", lng, episode.Number);

            var embed = new EmbedBuilder()
                .WithAuthor(title, url: project.AniListUrl)
                .WithTitle(embedTitle)
                .WithThumbnailUrl(project.PosterUri)
                .WithDescription(roster)
                .WithCurrentTimestamp()
                .Build();

            await interaction.FollowupAsync(embed: embed);

            return ExecutionResult.Success;
        }
    }
}
