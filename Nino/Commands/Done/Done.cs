using Discord;
using Discord.WebSocket;
using Nino.Records;
using Nino.Records.Enums;
using Nino.Utilities;
using NLog;
using static Localizer.Localizer;

namespace Nino.Commands
{
    internal static partial class Done
    {
        public const string Name = "done";
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public static async Task<bool> Handle(SocketSlashCommand interaction)
        {
            var lng = interaction.UserLocale;
            var alias = ((string)interaction.Data.Options.FirstOrDefault(o => o.Name == "project")!.Value).Trim();
            var abbreviation = ((string)interaction.Data.Options.FirstOrDefault(o => o.Name == "abbreviation")!.Value).Trim().ToUpperInvariant();
            var episodeValue = interaction.Data.Options.FirstOrDefault(o => o.Name == "episode")?.Value;
            
            // Verify project
            var project = Utils.ResolveAlias(alias, interaction);
            if (project == null)
                return await Response.Fail(T("error.alias.resolutionFailed", lng, alias), interaction);

            if (episodeValue != null)
                return await HandleSpecified(interaction, project, abbreviation);
            else
                return true;     
                // // Try and find the next applicable episode
                // var nextWorkingNumber = episodes.FirstOrDefault(e => !e.Done)?.Number ?? episodes.LastOrDefault()?.Number;
                // if (nextWorkingNumber == null)
                //     return await Response.Fail(T("error.noEpisodes", lng), interaction);
                // workingEpisodeNumber = (decimal)nextWorkingNumber;

                // var nextTaskNumber = episodes.FirstOrDefault(e => e.Tasks.Any(t => t.Abbreviation == abbreviation && !t.Done))?.Number;
                // if (nextTaskNumber == null)
                // {
                //     if (episodes.Any(e => e.Tasks.Any(t => t.Abbreviation == abbreviation)))
                //         return await Response.Fail(T("error.progress.taskAlreadyDone", lng, abbreviation), interaction);
                //     else
                //         return await Response.Fail(T("error.noSuchTask", lng, abbreviation), interaction);
                // }
        }

        public static SlashCommandBuilder Builder =>
            new SlashCommandBuilder()
            .WithName(Name)
            .WithDescription("Mark a position as done")
            .WithNameLocalizations(GetCommandNames(Name))
            .WithDescriptionLocalizations(GetCommandDescriptions(Name))
            .AddOption(CommonOptions.Project())
            .AddOption(CommonOptions.Abbreviation())
            .AddOption(CommonOptions.Episode(required: false));
    }
}
