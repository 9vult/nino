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
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        [SlashCommand("roster", "See who's working on an episode")]
        public async Task<RuntimeResult> Handle(
            [Summary("project", "Project nickname"), Autocomplete(typeof(ProjectAutocompleteHandler))] string alias,
            [Summary("episode", "Episode number"), Autocomplete(typeof(EpisodeAutocompleteHandler))] decimal episodeNumber
        )
        {
            var interaction = Context.Interaction;
            var lng = interaction.UserLocale;
            
            // Sanitize inputs
            alias = alias.Trim();
            
            // Verify project and user - minimum Key Staff required
            var project = Utils.ResolveAlias(alias, interaction);
            if (project == null)
                return await Response.Fail(T("error.alias.resolutionFailed", lng, alias), interaction);

            if (!Utils.VerifyUser(interaction.User.Id, project, includeKeyStaff: true))
                return await Response.Fail(T("error.permissionDenied", lng), interaction);

            // Verify episode
            var episode = await Getters.GetEpisode(project, episodeNumber);
            if (episode == null)
                return await Response.Fail(T("error.noSuchEpisode", lng, episodeNumber), interaction);

            if (project.KeyStaff.Length == 0)
                return await Response.Fail(T("error.noRoster", lng), interaction);

            var roster = StaffList.GenerateRoster(project, episode);
            var title = project.IsPrivate
                ? $"🔒 {project.Title} ({project.Type.ToFriendlyString()})"
                : $"{project.Title} ({project.Type.ToFriendlyString()})";

            var embed = new EmbedBuilder()
                .WithAuthor(title)
                .WithTitle(T("title.blamedEpisode", lng, episode.Number))
                .WithThumbnailUrl(project.PosterUri)
                .WithDescription(roster)
                .WithCurrentTimestamp()
                .Build();

            await interaction.FollowupAsync(embed: embed);

            return ExecutionResult.Success;
        }
    }
}
