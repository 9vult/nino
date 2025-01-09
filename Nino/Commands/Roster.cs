using Discord;
using Discord.Interactions;
using Nino.Handlers;
using Nino.Records.Enums;
using Nino.Utilities;
using NLog;
using static Localizer.Localizer;

namespace Nino.Commands
{
    public class Roster(InteractionHandler handler, InteractionService commands) : InteractionModuleBase<SocketInteractionContext>
    {
        public InteractionService Commands { get; private set; } = commands;
        private readonly InteractionHandler _handler = handler;
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        [SlashCommand("roster", "See who's working on an episode")]
        public async Task<RuntimeResult> Handle(
            [Summary("project", "Project nickname"), Autocomplete(typeof(ProjectAutocompleteHandler))] string alias,
            [Summary("episode", "Episode number"), Autocomplete(typeof(EpisodeAutocompleteHandler))] string episodeNumber,
            [Summary("weights", "Display task weights"), Autocomplete(typeof(EpisodeAutocompleteHandler))] bool withWeights = false
        )
        {
            var interaction = Context.Interaction;
            var lng = interaction.UserLocale;
            
            // Sanitize inputs
            alias = alias.Trim();
            episodeNumber = Utils.CanonicalizeEpisodeNumber(episodeNumber);
            
            // Verify project and user - minimum Key Staff required
            var project = Utils.ResolveAlias(alias, interaction);
            if (project == null)
                return await Response.Fail(T("error.alias.resolutionFailed", lng, alias), interaction);

            if (!Utils.VerifyUser(interaction.User.Id, project, includeKeyStaff: true))
                return await Response.Fail(T("error.permissionDenied", lng), interaction);

            // Verify episode
            if (!Getters.TryGetEpisode(project, episodeNumber, out var episode))
                return await Response.Fail(T("error.noSuchEpisode", lng, episodeNumber), interaction);
            
            Log.Trace($"Generating roster for {project} episode {episode} for M[{interaction.User.Id} (@{interaction.User.Username})]");

            if (project.KeyStaff.Length == 0)
                return await Response.Fail(T("error.noRoster", lng), interaction);

            var roster = StaffList.GenerateRoster(project, episode, withWeights);
            var title = project.IsPrivate
                ? $"🔒 {project.Title} ({project.Type.ToFriendlyString(lng)})"
                : $"{project.Title} ({project.Type.ToFriendlyString(lng)})";

            var embed = new EmbedBuilder()
                .WithAuthor(title)
                .WithTitle(T("title.progress", lng, episode.Number))
                .WithThumbnailUrl(project.PosterUri)
                .WithDescription(roster)
                .WithCurrentTimestamp()
                .Build();

            await interaction.FollowupAsync(embed: embed);

            return ExecutionResult.Success;
        }
    }
}
