using Discord;
using Discord.WebSocket;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static Localizer.Localizer;

namespace Nino.Commands
{
    internal static partial class KeyStaff
    {
        public const string Name = "keystaff";

        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public static async Task<bool> Handle(SocketSlashCommand interaction)
        {
            var subcommandName = interaction.Data.Options.First().Name;

            switch (subcommandName)
            {
                case "add":
                    return await HandleAdd(interaction);
                case "remove":
                    return await HandleRemove(interaction);
                case "swap":
                    return await HandleSwap(interaction);
                case "setweight":
                    return await HandleSetWeight(interaction);
                default:
                    log.Error($"Unknown KeyStaff subcommand {subcommandName}");
                    return false;
            }
        }

        public static SlashCommandBuilder Builder =>
            new SlashCommandBuilder()
            .WithName(Name)
            .WithDescription("Key Staff for the whole project")
            .WithNameLocalizations(GetCommandNames(Name))
            .WithDescriptionLocalizations(GetCommandDescriptions(Name))
            // Add
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("add")
                .WithDescription("Add a new Key Staff to the whole project")
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
                .WithDescription("Remove a new Key Staff from the whole project")
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
                .WithDescription("Swap a Key Staff for the whole project")
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
