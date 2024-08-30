using Discord;
using static Localizer.Localizer;

namespace Nino.Utilities
{
    internal static class CommonOptions
    {
        /// <summary>
        /// Project name input
        /// </summary>
        /// <param name="autocomplete">Is autocomplete enabled?</param>
        public static SlashCommandOptionBuilder Project(bool autocomplete = true) =>
            new SlashCommandOptionBuilder()
            .WithName("project")
            .WithDescription("Project nickname")
            .WithNameLocalizations(GetOptionNames("project"))
            .WithDescriptionLocalizations(GetOptionDescriptions("project"))
            .WithRequired(true)
            .WithAutocomplete(true)
            .WithType(ApplicationCommandOptionType.String);

        /// <summary>
        /// Member input
        /// </summary>
        public static SlashCommandOptionBuilder Member() =>
            new SlashCommandOptionBuilder()
            .WithName("member")
            .WithDescription("Staff member")
            .WithNameLocalizations(GetOptionNames("member"))
            .WithDescriptionLocalizations(GetOptionDescriptions("member"))
            .WithRequired(true)
            .WithType(ApplicationCommandOptionType.User);

        /// <summary>
        /// Episode number input
        /// </summary>
        /// <param name="autocomplete">Is autocomplete enabled?</param>
        public static SlashCommandOptionBuilder Episode(bool autocomplete = true, bool required = true) =>
            new SlashCommandOptionBuilder()
            .WithName("episode")
            .WithDescription("Episode number")
            .WithNameLocalizations(GetOptionNames("episode"))
            .WithDescriptionLocalizations(GetOptionDescriptions("episode"))
            .WithRequired(required)
            .WithAutocomplete(autocomplete)
            .WithType(ApplicationCommandOptionType.Number);

        /// <summary>
        /// Task abbreviation input
        /// </summary>
        /// <param name="autocomplete">Is autocomplete enabled?</param>
        public static SlashCommandOptionBuilder Abbreviation(bool autocomplete = true) =>
            new SlashCommandOptionBuilder()
            .WithName("abbreviation")
            .WithDescription("Position shorthand")
            .WithNameLocalizations(GetOptionNames("abbreviation"))
            .WithDescriptionLocalizations(GetOptionDescriptions("abbreviation"))
            .WithRequired(true)
            .WithAutocomplete(autocomplete)
            .WithType(ApplicationCommandOptionType.String);

    }
}
