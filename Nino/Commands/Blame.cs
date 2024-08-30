using Discord;
using Discord.WebSocket;
using Nino.Records.Enums;
using Nino.Utilities;
using static Localizer.Localizer;

namespace Nino.Commands
{
    internal static partial class Blame
    {
        public const string Name = "blame";

        public static async Task<bool> Handle(SocketSlashCommand interaction)
        {
            var lng = interaction.UserLocale;
            var alias = ((string)interaction.Data.Options.FirstOrDefault(o => o.Name == "project")!.Value).Trim();
            var episodeValue = interaction.Data.Options.FirstOrDefault(o => o.Name == "episode")?.Value;
            var explain = (bool?)interaction.Data.Options.FirstOrDefault(o => o.Name == "explain")?.Value ?? false;
            
            // Verify project
            var project = Utils.ResolveAlias(alias, interaction);
            if (project == null)
                return await Response.Fail(T("error.alias.resolutionFailed", lng, alias), interaction);

            var episodes = Cache.GetEpisodes(project.Id).OrderBy(e => e.Number);

            // Verify or find episode
            decimal episodeNumber;
            if (episodeValue != null)
            {
                episodeNumber = Convert.ToDecimal(episodeValue);
            }
            else
            {
                var nextNumber = episodes.FirstOrDefault(e => !e.Done)?.Number ?? episodes.LastOrDefault()?.Number;
                if (nextNumber == null)
                    return await Response.Fail(T("error.noEpisodes", lng), interaction);
                episodeNumber = (decimal)nextNumber;
            }
            
            var episode = await Getters.GetEpisode(project, episodeNumber);
            if (episode == null)
                return await Response.Fail(T("error.noSuchEpisode", lng, episodeNumber), interaction);

            var progress = explain ? StaffList.GenerateExplainProgress(project, episode, lng)
                : StaffList.GenerateProgress(project, episode);

            var title = project.IsPrivate
                ? $"🔒 {project.Title} ({project.Type.ToFriendlyString()})"
                : $"{project.Title} ({project.Type.ToFriendlyString()})";

            var embed = new EmbedBuilder()
                .WithAuthor(title)
                .WithTitle(T("title.blamedEpisode", lng, episode.Number))
                .WithThumbnailUrl(project.PosterUri)
                .WithDescription(progress)
                .WithCurrentTimestamp()
                .Build();

            await interaction.FollowupAsync(embed: embed);

            return true;
        }

        public static SlashCommandBuilder Builder =>
            new SlashCommandBuilder()
            .WithName(Name)
            .WithDescription("Check the status of a project")
            .WithNameLocalizations(GetCommandNames(Name))
            .WithDescriptionLocalizations(GetCommandDescriptions(Name))
            .AddOption(CommonOptions.Project())
            .AddOption(CommonOptions.Episode(required: false))
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("explain")
                .WithDescription("Explain what any of this means")
                .WithNameLocalizations(GetOptionNames("blame.explain"))
                .WithDescriptionLocalizations(GetOptionDescriptions("blame.explain"))
                .WithRequired(false)
                .WithType(ApplicationCommandOptionType.Boolean)    
            );
    }
}
