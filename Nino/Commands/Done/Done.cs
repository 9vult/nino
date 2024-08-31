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
                return await HandleSpecified(interaction, project, abbreviation, Convert.ToDecimal(episodeValue));
            else
                return await HandleUnspecified(interaction, project, abbreviation);
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
