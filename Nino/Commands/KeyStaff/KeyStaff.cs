﻿using Discord;
using Discord.WebSocket;
using Nino.Utilities;
using NLog;

using static Localizer.Localizer;

namespace Nino.Commands
{
    internal static partial class KeyStaff
    {
        public const string Name = "keystaff";

        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public static async Task<bool> Handle(SocketSlashCommand interaction)
        {
            var lng = interaction.UserLocale;
            var subcommand = interaction.Data.Options.First();

            var alias = ((string)subcommand.Options.FirstOrDefault(o => o.Name == "project")!.Value).Trim();

            // Verify project and user - Owner or Admin required
            var project = Utils.ResolveAlias(alias, interaction);
            if (project == null)
                return await Response.Fail(T("error.alias.resolutionFailed", lng, alias), interaction);

            if (!Utils.VerifyUser(interaction.User.Id, project))
                return await Response.Fail(T("error.permissionDenied", lng), interaction);

            switch (subcommand.Name)
            {
                case "add":
                    return await HandleAdd(interaction, project);
                case "remove":
                    return await HandleRemove(interaction, project);
                case "swap":
                    return await HandleSwap(interaction, project);
                case "setweight":
                    return await HandleSetWeight(interaction, project);
                default:
                    log.Error($"Unknown KeyStaff subcommand {subcommand.Name}");
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
                .AddOption(CommonOptions.Project())
                .AddOption(CommonOptions.Member())
                .AddOption(CommonOptions.Abbreviation(false))
                .AddOption(new SlashCommandOptionBuilder()
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
                .AddOption(CommonOptions.Project())
                .AddOption(CommonOptions.Abbreviation(false))
            )
            // Swap
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("swap")
                .WithDescription("Swap a Key Staff for the whole project")
                .WithNameLocalizations(GetCommandNames("keystaff.swap"))
                .WithDescriptionLocalizations(GetCommandDescriptions("keystaff.swap"))
                .WithType(ApplicationCommandOptionType.SubCommand)
                .AddOption(CommonOptions.Project())
                .AddOption(CommonOptions.Abbreviation(false))
                .AddOption(CommonOptions.Member())
            )
            // Set Weight
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("setweight")
                .WithDescription("Set the weight of a Key Staff position")
                .WithNameLocalizations(GetCommandNames("keystaff.setweight"))
                .WithDescriptionLocalizations(GetCommandDescriptions("keystaff.setweight"))
                .WithType(ApplicationCommandOptionType.SubCommand)
                .AddOption(CommonOptions.Project())
                .AddOption(CommonOptions.Abbreviation(false))
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
