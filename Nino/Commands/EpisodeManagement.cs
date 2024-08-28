using Discord;
using Discord.WebSocket;
using Nino.Utilities;
using NLog;

using static Localizer.Localizer;

namespace Nino.Commands
{
    internal static partial class EpisodeManagement
    {
        public const string Name = "episode";

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
                    log.Error($"Unknown Episode subcommand {subcommand.Name}");
                    return false;
            }
        }

        public static SlashCommandBuilder Builder =>
            new SlashCommandBuilder()
            .WithName(Name)
            .WithDescription("Episodes")
            .WithNameLocalizations(GetCommandNames(Name))
            .WithDescriptionLocalizations(GetCommandDescriptions(Name))
            // Add
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("add")
                .WithDescription("Add an episode")
                .WithNameLocalizations(GetCommandNames("episode.add"))
                .WithDescriptionLocalizations(GetCommandDescriptions("episode.add"))
                .WithType(ApplicationCommandOptionType.SubCommand)
                .AddOption(CommonOptions.Project())
                .AddOption(CommonOptions.Episode(false))
            )
            // Remove
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("remove")
                .WithDescription("Remove an episode")
                .WithNameLocalizations(GetCommandNames("episode.remove"))
                .WithDescriptionLocalizations(GetCommandDescriptions("episode.remove"))
                .WithType(ApplicationCommandOptionType.SubCommand)
                .AddOption(CommonOptions.Project())
                .AddOption(CommonOptions.Episode())
            );
    }
}
