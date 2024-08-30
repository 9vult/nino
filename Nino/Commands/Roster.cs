using Discord;
using Discord.WebSocket;
using Nino.Records.Enums;
using Nino.Utilities;
using static Localizer.Localizer;

namespace Nino.Commands
{
    internal static partial class Roster
    {
        public const string Name = "roster";

        public static async Task<bool> Handle(SocketSlashCommand interaction)
        {
            var lng = interaction.UserLocale;
            var alias = ((string)interaction.Data.Options.FirstOrDefault(o => o.Name == "project")!.Value).Trim();
            
            // Verify project and user - minimum Key Staff required
            var project = Utils.ResolveAlias(alias, interaction);
            if (project == null)
                return await Response.Fail(T("error.alias.resolutionFailed", lng, alias), interaction);

            if (!Utils.VerifyUser(interaction.User.Id, project, includeKeyStaff: true))
                return await Response.Fail(T("error.permissionDenied", lng), interaction);

            // Verify episode
            var episodeNumber = Convert.ToDecimal(interaction.Data.Options.FirstOrDefault(o => o.Name == "episode")!.Value);
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

            return true;
        }

        public static SlashCommandBuilder Builder =>
            new SlashCommandBuilder()
            .WithName(Name)
            .WithDescription("See who's working on an episode")
            .WithNameLocalizations(GetCommandNames(Name))
            .WithDescriptionLocalizations(GetCommandDescriptions(Name))
            .AddOption(CommonOptions.Project())
            .AddOption(CommonOptions.Episode());
    }
}
