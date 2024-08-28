using Discord;
using Discord.WebSocket;
using Nino.Utilities;
using NLog;

using static Localizer.Localizer;

namespace Nino.Commands
{
    internal static partial class ServerManagement
    {
        public const string Name = "server";

        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public static async Task<bool> Handle(SocketSlashCommand interaction)
        {
            var guildId = interaction.GuildId ?? 0;
            var guild = Nino.Client.GetGuild(guildId);
            var member = guild.GetUser(interaction.User.Id);
            var lng = interaction.UserLocale;

            // Server administrator permissions required
            if (!member.GuildPermissions.Administrator)
                return await Response.Fail(T("error.notPrivileged", lng), interaction);

            var config = await Getters.GetConfiguration(guildId);
            if (config == null)
                return await Response.Fail(T("error.noSuchConfig", lng), interaction);

            var subcommand = interaction.Data.Options.First();
            switch (subcommand.Name)
            {
                case "releaseprefix":
                    return await HandleSetReleasePrefix(interaction, config);
                case "display":
                    var subsubcommand = subcommand.Options.First();
                    switch (subsubcommand.Name)
                    {
                        case "updates":
                            return await HandleSetDisplayUpdates(interaction, config);
                        case "progress":
                            return  await HandleSetDisplayProgress(interaction, config);
                    }
                    log.Error($"Unknown ServerManagement/Display subcommand {subsubcommand.Name}");
                    return false;
                case "admin":
                    subsubcommand = subcommand.Options.First();
                    switch (subsubcommand.Name)
                    {
                        case "add":
                            return await HandleAdminAdd(interaction, config);
                        case "remove":
                            return await HandleAdminRemove(interaction, config);
                    }
                    log.Error($"Unknown ServerManagement/Admin subcommand {subsubcommand.Name}");
                    return false;
                default:
                    log.Error($"Unknown ServerManagement subcommand {subcommand.Name}");
                    return false;
            }
        }

        public static SlashCommandBuilder Builder =>
            new SlashCommandBuilder()
            .WithName(Name)
            .WithDescription("Server-wide options")
            .WithNameLocalizations(GetCommandNames(Name))
            .WithDescriptionLocalizations(GetCommandDescriptions(Name))
            .AddOption(DisplaySubcommandGroup)
            .AddOption(AdminSubcommandGroup)
            .AddOption(ReleasePrefixSubcommand);

        private static SlashCommandOptionBuilder DisplaySubcommandGroup =>
            new SlashCommandOptionBuilder()
            .WithName("display")
            .WithDescription("Control the look and features of displays")
            .WithNameLocalizations(GetCommandNames("server.display"))
            .WithDescriptionLocalizations(GetCommandDescriptions("server.display"))
            .WithType(ApplicationCommandOptionType.SubCommandGroup)
            .AddOption(UpdatesDisplaySubcommand)
            .AddOption(ProgressDisplaySubcommand);

        private static SlashCommandOptionBuilder UpdatesDisplaySubcommand =>
            new SlashCommandOptionBuilder()
            .WithName("updates")
            .WithDescription("Control how progress updates should look")
            .WithNameLocalizations(GetCommandNames("server.display.updates"))
            .WithDescriptionLocalizations(GetCommandDescriptions("server.display.updates"))
            .WithType(ApplicationCommandOptionType.SubCommand)
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("type")
                .WithDescription("Embed type")
                .WithNameLocalizations(GetOptionNames("server.display.type"))
                .WithDescriptionLocalizations(GetOptionDescriptions("server.display.type"))
                .AddChoice("Normal", 0, GetChoiceNames("server.display.type.normal"))
                .AddChoice("Extended", 1, GetChoiceNames("server.display.type.extended"))
                .WithRequired(true)
                .WithType(ApplicationCommandOptionType.Number)
            );

        private static SlashCommandOptionBuilder ProgressDisplaySubcommand =>
            new SlashCommandOptionBuilder()
            .WithName("progress")
            .WithDescription("Control how progress command responses should look")
            .WithNameLocalizations(GetCommandNames("server.display.progress"))
            .WithDescriptionLocalizations(GetCommandDescriptions("server.display.progress"))
            .WithType(ApplicationCommandOptionType.SubCommand)
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("type")
                .WithDescription("Embed type")
                .WithNameLocalizations(GetOptionNames("server.display.type"))
                .WithDescriptionLocalizations(GetOptionDescriptions("server.display.type"))
                .AddChoice("Succinct", 2, GetChoiceNames("server.display.type.succinct"))
                .AddChoice("Verbose", 3, GetChoiceNames("server.display.type.verbose"))
                .WithRequired(true)
                .WithType(ApplicationCommandOptionType.Number)
            );

        private static SlashCommandOptionBuilder ReleasePrefixSubcommand =>
            new SlashCommandOptionBuilder()
            .WithName("releaseprefix")
            .WithDescription("Specify a prefix for releases")
            .WithNameLocalizations(GetCommandNames("server.releaseprefix"))
            .WithDescriptionLocalizations(GetCommandDescriptions("server.releaseprefix"))
            .WithType(ApplicationCommandOptionType.SubCommand)
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("newvalue")
                .WithDescription("New value")
                .WithNameLocalizations(GetOptionNames("newvalue"))
                .WithDescriptionLocalizations(GetOptionDescriptions("newvalue"))
                .WithRequired(true)
                .WithType(ApplicationCommandOptionType.String)
            );

        private static SlashCommandOptionBuilder AdminSubcommandGroup =>
            new SlashCommandOptionBuilder()
            .WithName("admin")
            .WithDescription("Server-level administrators")
            .WithNameLocalizations(GetCommandNames("server.admin"))
            .WithDescriptionLocalizations(GetCommandDescriptions("server.admin"))
            .WithType(ApplicationCommandOptionType.SubCommandGroup)
            .AddOption(AddAdminSubcommand)
            .AddOption(RemoveAdminSubcommand);

        private static SlashCommandOptionBuilder AddAdminSubcommand =>
            new SlashCommandOptionBuilder()
            .WithName("add")
            .WithDescription("Add an administrator to this server")
            .WithNameLocalizations(GetCommandNames("server.admin.add"))
            .WithDescriptionLocalizations(GetCommandDescriptions("server.admin.add"))
            .WithType(ApplicationCommandOptionType.SubCommand)
            .AddOption(CommonOptions.Member());

        private static SlashCommandOptionBuilder RemoveAdminSubcommand =>
            new SlashCommandOptionBuilder()
            .WithName("remove")
            .WithDescription("Remove an administrator from this server")
            .WithNameLocalizations(GetCommandNames("server.admin.remove"))
            .WithDescriptionLocalizations(GetCommandDescriptions("server.admin.remove"))
            .WithType(ApplicationCommandOptionType.SubCommand)
            .AddOption(CommonOptions.Member());

    }
}
