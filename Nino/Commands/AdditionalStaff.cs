using Discord;
using Discord.WebSocket;
using Nino.Utilities;
using NLog;

using static Localizer.Localizer;

namespace Nino.Commands
{
    internal static partial class AdditionalStaff
    {
        public const string Name = "additionalstaff";

        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public static async Task<bool> Handle(SocketSlashCommand interaction)
        {
            var lng = interaction.UserLocale;
            var subcommand = interaction.Data.Options.First();

            var alias = ((string)subcommand.Options.FirstOrDefault(o => o.Name == "project")!.Value).Trim();

            // Verify project and user - Owner or Admin required
            var project = await Utils.ResolveAlias(alias, interaction);
            if (project == null)
                return await Response.Fail(T("error.alias.resolutionFailed", lng, alias), interaction);

            if (!Utils.VerifyUser(interaction.User.Id, project))
                return await Response.Fail(T("error.permissionDenied", lng), interaction);

            // Verify episode
            var episodeNumber = Convert.ToDecimal(subcommand.Options.FirstOrDefault(o => o.Name == "episode")!.Value);
            var episode = await Getters.GetEpisode(project, episodeNumber);

            if (episode == null)
                return await Response.Fail(T("error.noSuchEpisode", lng, episodeNumber), interaction);

            switch (subcommand.Name)
            {
                case "add":
                    return await HandleAdd(interaction, episode);
                case "remove":
                    return await HandleRemove(interaction, episode);
                case "swap":
                    //return await HandleSwap(interaction, episode);
                default:
                    log.Error($"Unknown AdditionalStaff subcommand {subcommand.Name}");
                    return false;
            }
        }

        public static SlashCommandBuilder Builder =>
            new SlashCommandBuilder()
            .WithName(Name)
            .WithDescription("Additional staff for a single episode")
            .WithNameLocalizations(GetCommandNames(Name))
            .WithDescriptionLocalizations(GetCommandDescriptions(Name))
            // Add
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("add")
                .WithDescription("Add additional staff to an episode")
                .WithNameLocalizations(GetCommandNames("keystaff.add"))
                .WithDescriptionLocalizations(GetCommandDescriptions("keystaff.add"))
                .WithType(ApplicationCommandOptionType.SubCommand)
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("project")
                    .WithDescription("Project nickname")
                    .WithNameLocalizations(GetOptionNames("project"))
                    .WithDescriptionLocalizations(GetOptionDescriptions("project"))
                    .WithRequired(true)
                    .WithAutocomplete(true)
                    .WithType(ApplicationCommandOptionType.String)
                ).AddOption(new SlashCommandOptionBuilder()
                    .WithName("episode")
                    .WithDescription("Episode number")
                    .WithNameLocalizations(GetOptionNames("episode"))
                    .WithDescriptionLocalizations(GetOptionDescriptions("episode"))
                    .WithRequired(true)
                    .WithAutocomplete(true)
                    .WithType(ApplicationCommandOptionType.Number)
                ).AddOption(new SlashCommandOptionBuilder()
                    .WithName("member")
                    .WithDescription("Staff member")
                    .WithNameLocalizations(GetOptionNames("member"))
                    .WithDescriptionLocalizations(GetOptionDescriptions("member"))
                    .WithRequired(true)
                    .WithType(ApplicationCommandOptionType.User)
                ).AddOption(new SlashCommandOptionBuilder()
                    .WithName("abbreviation")
                    .WithDescription("Position shorthand")
                    .WithNameLocalizations(GetOptionNames("abbreviation"))
                    .WithDescriptionLocalizations(GetOptionDescriptions("abbreviation"))
                    .WithRequired(true)
                    .WithType(ApplicationCommandOptionType.String)
                ).AddOption(new SlashCommandOptionBuilder()
                    .WithName("name")
                    .WithDescription("Full position name")
                    .WithNameLocalizations(GetOptionNames("name"))
                    .WithDescriptionLocalizations(GetOptionDescriptions("name"))
                    .WithRequired(true)
                    .WithType(ApplicationCommandOptionType.String)
                )
            )
            // Remove
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("remove")
                .WithDescription("Remove additional staff from an episode")
                .WithNameLocalizations(GetCommandNames("keystaff.remove"))
                .WithDescriptionLocalizations(GetCommandDescriptions("keystaff.remove"))
                .WithType(ApplicationCommandOptionType.SubCommand)
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("project")
                    .WithDescription("Project nickname")
                    .WithNameLocalizations(GetOptionNames("project"))
                    .WithDescriptionLocalizations(GetOptionDescriptions("project"))
                    .WithRequired(true)
                    .WithAutocomplete(true)
                    .WithType(ApplicationCommandOptionType.String)
                ).AddOption(new SlashCommandOptionBuilder()
                    .WithName("episode")
                    .WithDescription("Episode number")
                    .WithNameLocalizations(GetOptionNames("episode"))
                    .WithDescriptionLocalizations(GetOptionDescriptions("episode"))
                    .WithRequired(true)
                    .WithAutocomplete(true)
                    .WithType(ApplicationCommandOptionType.Number)
                ).AddOption(new SlashCommandOptionBuilder()
                    .WithName("abbreviation")
                    .WithDescription("Position shorthand")
                    .WithNameLocalizations(GetOptionNames("abbreviation"))
                    .WithDescriptionLocalizations(GetOptionDescriptions("abbreviation"))
                    .WithRequired(true)
                    .WithType(ApplicationCommandOptionType.String)
                )
            )
            // Swap
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("swap")
                .WithDescription("Swap additional staff into an episode")
                .WithNameLocalizations(GetCommandNames("keystaff.swap"))
                .WithDescriptionLocalizations(GetCommandDescriptions("keystaff.swap"))
                .WithType(ApplicationCommandOptionType.SubCommand)
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("project")
                    .WithDescription("Project nickname")
                    .WithNameLocalizations(GetOptionNames("project"))
                    .WithDescriptionLocalizations(GetOptionDescriptions("project"))
                    .WithRequired(true)
                    .WithAutocomplete(true)
                    .WithType(ApplicationCommandOptionType.String)
                ).AddOption(new SlashCommandOptionBuilder()
                    .WithName("episode")
                    .WithDescription("Episode number")
                    .WithNameLocalizations(GetOptionNames("episode"))
                    .WithDescriptionLocalizations(GetOptionDescriptions("episode"))
                    .WithRequired(true)
                    .WithAutocomplete(true)
                    .WithType(ApplicationCommandOptionType.Number)
                ).AddOption(new SlashCommandOptionBuilder()
                    .WithName("abbreviation")
                    .WithDescription("Position shorthand")
                    .WithNameLocalizations(GetOptionNames("abbreviation"))
                    .WithDescriptionLocalizations(GetOptionDescriptions("abbreviation"))
                    .WithRequired(true)
                    .WithType(ApplicationCommandOptionType.String)
                ).AddOption(new SlashCommandOptionBuilder()
                    .WithName("member")
                    .WithDescription("Staff member")
                    .WithNameLocalizations(GetOptionNames("member"))
                    .WithDescriptionLocalizations(GetOptionDescriptions("member"))
                    .WithRequired(true)
                    .WithType(ApplicationCommandOptionType.User)
                )
            )
            // Set Weight
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("setweight")
                .WithDescription("Set the weight of a Key Staff position")
                .WithNameLocalizations(GetCommandNames("keystaff.setweight"))
                .WithDescriptionLocalizations(GetCommandDescriptions("keystaff.setweight"))
                .WithType(ApplicationCommandOptionType.SubCommand)
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("project")
                    .WithDescription("Project nickname")
                    .WithNameLocalizations(GetOptionNames("project"))
                    .WithDescriptionLocalizations(GetOptionDescriptions("project"))
                    .WithRequired(true)
                    .WithAutocomplete(true)
                    .WithType(ApplicationCommandOptionType.String)
                ).AddOption(new SlashCommandOptionBuilder()
                    .WithName("abbreviation")
                    .WithDescription("Position shorthand")
                    .WithNameLocalizations(GetOptionNames("abbreviation"))
                    .WithDescriptionLocalizations(GetOptionDescriptions("abbreviation"))
                    .WithRequired(true)
                    .WithType(ApplicationCommandOptionType.String)
                )
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("weight")
                    .WithDescription("Weight")
                    .WithNameLocalizations(GetOptionNames("weight"))
                    .WithDescriptionLocalizations(GetOptionDescriptions("weight"))
                    .WithRequired(true)
                    .WithType(ApplicationCommandOptionType.Number)
                )
            );
    }
}
