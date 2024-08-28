using Discord;
using Discord.WebSocket;
using Nino.Utilities;
using NLog;

using static Localizer.Localizer;

namespace Nino.Commands
{
    internal static partial class Observer
    {
        public const string Name = "observer";

        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public static async Task<bool> Handle(SocketSlashCommand interaction)
        {
            var subcommand = interaction.Data.Options.First();

            switch (subcommand.Name)
            {
                case "add":
                    return await HandleAdd(interaction);
                case "remove":
                    return await HandleRemove(interaction);
                default:
                    log.Error($"Unknown Observer subcommand {subcommand.Name}");
                    return false;
            }
        }

        public static SlashCommandBuilder Builder =>
            new SlashCommandBuilder()
            .WithName(Name)
            .WithDescription("Observe projects on another server")
            .WithNameLocalizations(GetCommandNames(Name))
            .WithDescriptionLocalizations(GetCommandDescriptions(Name))
            // Add
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("add")
                .WithDescription("Start observing a project on another server")
                .WithNameLocalizations(GetCommandNames("observer.add"))
                .WithDescriptionLocalizations(GetCommandDescriptions("observer.add"))
                .WithType(ApplicationCommandOptionType.SubCommand)
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("serverid")
                    .WithDescription("ID of the server you want to observe")
                    .WithNameLocalizations(GetOptionNames("observer.serverid"))
                    .WithDescriptionLocalizations(GetOptionDescriptions("observer.serverid"))
                    .WithRequired(true)
                    .WithType(ApplicationCommandOptionType.String)
                ).AddOption(CommonOptions.Project(false))
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("blame")
                    .WithDescription("Should this project's aliases show up in /blame?")
                    .WithNameLocalizations(GetOptionNames("observer.blame"))
                    .WithDescriptionLocalizations(GetOptionDescriptions("observer.blame"))
                    .WithRequired(true)
                    .WithType(ApplicationCommandOptionType.Boolean)
                ).AddOption(new SlashCommandOptionBuilder()
                    .WithName("updates")
                    .WithDescription("Webhook URL for progress updates")
                    .WithNameLocalizations(GetOptionNames("observer.updates"))
                    .WithDescriptionLocalizations(GetOptionDescriptions("observer.updates"))
                    .WithRequired(false)
                    .WithType(ApplicationCommandOptionType.String)
                ).AddOption(new SlashCommandOptionBuilder()
                    .WithName("releases")
                    .WithDescription("Webhook URL for releases")
                    .WithNameLocalizations(GetOptionNames("observer.releases"))
                    .WithDescriptionLocalizations(GetOptionDescriptions("observer.releases"))
                    .WithRequired(false)
                    .WithType(ApplicationCommandOptionType.String)
                ).AddOption(new SlashCommandOptionBuilder()
                    .WithName("role")
                    .WithDescription("Role to ping for releases")
                    .WithNameLocalizations(GetOptionNames("observer.role"))
                    .WithDescriptionLocalizations(GetOptionDescriptions("observer.role"))
                    .WithRequired(false)
                    .WithType(ApplicationCommandOptionType.Role)
                )
            )
            // Remove
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("remove")
                .WithDescription("Stop observing a project on another server")
                .WithNameLocalizations(GetCommandNames("observer.remove"))
                .WithDescriptionLocalizations(GetCommandDescriptions("observer.remove"))
                .WithType(ApplicationCommandOptionType.SubCommand)
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("serverid")
                    .WithDescription("ID of the server you want to observe")
                    .WithNameLocalizations(GetOptionNames("observer.serverid"))
                    .WithDescriptionLocalizations(GetOptionDescriptions("observer.serverid"))
                    .WithRequired(true)
                    .WithType(ApplicationCommandOptionType.String)
                ).AddOption(CommonOptions.Project(false))
            );
    }
}
