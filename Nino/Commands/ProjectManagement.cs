using Discord;
using Discord.WebSocket;
using Nino.Utilities;
using NLog;

using static Localizer.Localizer;

namespace Nino.Commands
{
    internal static partial class ProjectManagement
    {
        public const string Name = "project";

        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public static async Task<bool> Handle(SocketSlashCommand interaction)
        {
            var subcommand = interaction.Data.Options.First();

            switch (subcommand.Name)
            {
                case "create":
                    return await HandleCreate(interaction);
                case "edit":
                    return await HandleEdit(interaction);
                case "delete":
                    return await HandleDelete(interaction);
                case "transferownership":
                    return await HandleTransferOwnership(interaction);
                case "alias":
                    var subsubcommand = subcommand.Options.First();
                    switch (subsubcommand.Name)
                    {
                        case "add":
                            return await HandleAliasAdd(interaction);
                        case "remove":
                            return await HandleAliasRemove(interaction);
                    }
                    log.Error($"Unknown ProjectManagement/Alias subcommand {subsubcommand.Name}");
                    return false;
                case "admin":
                    subsubcommand = subcommand.Options.First();
                    switch (subsubcommand.Name)
                    {
                        case "add":
                            return await HandleAdminAdd(interaction);
                        case "remove":
                            return await HandleAdminRemove(interaction);
                    }
                    log.Error($"Unknown ProjectManagement/Admin subcommand {subsubcommand.Name}");
                    return false;
                case "conga":
                    subsubcommand = subcommand.Options.First();
                    switch (subsubcommand.Name)
                    {
                        case "add":
                            return await HandleCongaAdd(interaction);
                        case "remove":
                            return await HandleCongaRemove(interaction);
                        case "list":
                            return await HandleCongaList(interaction);
                    }
                    log.Error($"Unknown ProjectManagement/Conga subcommand {subsubcommand.Name}");
                    return false;
                case "airreminder":
                    subsubcommand = subcommand.Options.First();
                    switch (subsubcommand.Name)
                    {
                        case "enable":
                            return await HandleAirReminderEnable(interaction);
                        case "disable":
                            return await HandleAirReminderDisable(interaction);
                    }
                    log.Error($"Unknown ProjectManagement/Admin subcommand {subsubcommand.Name}");
                    return false;
                default:
                    log.Error($"Unknown ProjectManagement subcommand {subcommand.Name}");
                    return false;
            }
        }

        public static SlashCommandBuilder Builder =>
            new SlashCommandBuilder()
            .WithName(Name)
            .WithNameLocalizations(GetCommandNames(Name))
            .WithDescription("Create a new project")
            .WithDescriptionLocalizations(GetCommandDescriptions(Name))
            .AddOption(CreateSubcommand)
            .AddOption(EditSubcommand)
            .AddOption(DeleteSubcommand)
            .AddOption(TransferOwnershipSubcommand)
            .AddOption(AliasSubcommandGroup
                .AddOption(AddAliasSubcommand)
                .AddOption(RemoveAliasSubcommand)
            )
            .AddOption(AdminSubcommandGroup
                .AddOption(AddAdminSubcommand)
                .AddOption(RemoveAdminSubcommand)
            )
            .AddOption(CongaSubcommandGroup
                .AddOption(AddCongaSubcommand)
                .AddOption(RemoveCongaSubcommand)
                .AddOption(ListCongaSubcommand)
            ).AddOption(AirReminderSubcommandGroup
                .AddOption(AirReminderEnableSubcommand)
                .AddOption(AirReminderDisableSubcommand)
            );

        private static SlashCommandOptionBuilder CreateSubcommand =>
            new SlashCommandOptionBuilder()
            .WithName("create")
            .WithDescription("Create a new project")
            .WithNameLocalizations(GetCommandNames("project.create"))
            .WithDescriptionLocalizations(GetCommandDescriptions("project.create"))
            .WithType(ApplicationCommandOptionType.SubCommand)
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("nickname")
                .WithDescription("Project nickname")
                .WithNameLocalizations(GetOptionNames("project.create.nickname"))
                .WithDescriptionLocalizations(GetOptionDescriptions("project.create.nickname"))
                .WithRequired(true)
                .WithType(ApplicationCommandOptionType.String)
            ).AddOption(new SlashCommandOptionBuilder()
                .WithName("title")
                .WithDescription("Full series title")
                .WithNameLocalizations(GetOptionNames("project.create.title"))
                .WithDescriptionLocalizations(GetOptionDescriptions("project.create.title"))
                .WithRequired(true)
                .WithType(ApplicationCommandOptionType.String)
            ).AddOption(new SlashCommandOptionBuilder()
                .WithName("type")
                .WithDescription("Project type")
                .WithNameLocalizations(GetOptionNames("project.create.type"))
                .WithDescriptionLocalizations(GetOptionDescriptions("project.create.type"))
                .WithRequired(true)
                .AddChoice("TV", 0, GetChoiceNames("project.create.type.tv"))
                .AddChoice("Movie", 1, GetChoiceNames("project.create.type.movie"))
                .AddChoice("BD", 2, GetChoiceNames("project.create.type.bd"))
                .WithType(ApplicationCommandOptionType.Number)
            ).AddOption(new SlashCommandOptionBuilder()
                .WithName("length")
                .WithDescription("Number of episodes")
                .WithNameLocalizations(GetOptionNames("project.create.length"))
                .WithDescriptionLocalizations(GetOptionDescriptions("project.create.length"))
                .WithRequired(true)
                .WithMinValue(1)
                .WithType(ApplicationCommandOptionType.Number)
            ).AddOption(new SlashCommandOptionBuilder()
                .WithName("poster")
                .WithDescription("Poster image URL")
                .WithNameLocalizations(GetOptionNames("project.create.poster"))
                .WithDescriptionLocalizations(GetOptionDescriptions("project.create.poster"))
                .WithRequired(true)
                .WithType(ApplicationCommandOptionType.String)
            ).AddOption(new SlashCommandOptionBuilder()
                .WithName("private")
                .WithDescription("Is this project private?")
                .WithNameLocalizations(GetOptionNames("project.create.private"))
                .WithDescriptionLocalizations(GetOptionDescriptions("project.create.private"))
                .WithRequired(true)
                .WithType(ApplicationCommandOptionType.Boolean)
            ).AddOption(new SlashCommandOptionBuilder()
                .WithName("updatechannel")
                .WithDescription("Channel to post updates to")
                .WithNameLocalizations(GetOptionNames("project.create.updatechannel"))
                .WithDescriptionLocalizations(GetOptionDescriptions("project.create.updatechannel"))
                .WithRequired(true)
                .WithType(ApplicationCommandOptionType.Channel)
            ).AddOption(new SlashCommandOptionBuilder()
                .WithName("releasechannel")
                .WithDescription("Channel to post releases to")
                .WithNameLocalizations(GetOptionNames("project.create.releasechannel"))
                .WithDescriptionLocalizations(GetOptionDescriptions("project.create.releasechannel"))
                .WithRequired(true)
                .WithType(ApplicationCommandOptionType.Channel)
            );

        private static SlashCommandOptionBuilder EditSubcommand =>
            new SlashCommandOptionBuilder()
            .WithName("edit")
            .WithDescription("Edit a project")
            .WithNameLocalizations(GetCommandNames("project.edit"))
            .WithDescriptionLocalizations(GetCommandDescriptions("project.edit"))
            .WithType(ApplicationCommandOptionType.SubCommand)
            .AddOption(CommonOptions.Project())
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("option")
                .WithDescription("Option to change")
                .WithNameLocalizations(GetOptionNames("project.edit.option"))
                .WithDescriptionLocalizations(GetOptionDescriptions("project.edit.option"))
                .WithRequired(true)
                .AddChoice("Title", 0, GetChoiceNames("project.edit.option.title"))
                .AddChoice("Poster", 1, GetChoiceNames("project.edit.option.poster"))
                .AddChoice("MOTD", 2, GetChoiceNames("project.edit.option.motd"))
                .AddChoice("AniDBId", 3, GetChoiceNames("project.edit.option.anidb"))
                .AddChoice("AirTime24h", 4, GetChoiceNames("project.edit.option.airtime"))
                .AddChoice("IsPrivate", 5, GetChoiceNames("project.edit.option.private"))
                .AddChoice("UpdateChannelID", 6, GetChoiceNames("project.edit.option.updatechannel"))
                .AddChoice("ReleaseChannelID", 7, GetChoiceNames("project.edit.option.releasechannel"))
                .WithType(ApplicationCommandOptionType.Number)
            ).AddOption(new SlashCommandOptionBuilder()
                .WithName("newvalue")
                .WithDescription("New Value")
                .WithNameLocalizations(GetOptionNames("newvalue"))
                .WithDescriptionLocalizations(GetOptionDescriptions("newvalue"))
                .WithRequired(true)
                .WithType(ApplicationCommandOptionType.String)
            );

        private static SlashCommandOptionBuilder DeleteSubcommand =>
            new SlashCommandOptionBuilder()
            .WithName("delete")
            .WithDescription("Delete a project")
            .WithNameLocalizations(GetCommandNames("project.delete"))
            .WithDescriptionLocalizations(GetCommandDescriptions("project.delete"))
            .WithType(ApplicationCommandOptionType.SubCommand)
            .AddOption(CommonOptions.Project());

        private static SlashCommandOptionBuilder TransferOwnershipSubcommand =>
            new SlashCommandOptionBuilder()
            .WithName("transferownership")
            .WithDescription("Transfer project ownership to someone else")
            .WithNameLocalizations(GetCommandNames("project.transferownership"))
            .WithDescriptionLocalizations(GetCommandDescriptions("project.transferownership"))
            .WithType(ApplicationCommandOptionType.SubCommand)
            .AddOption(CommonOptions.Project())
            .AddOption(CommonOptions.Member());

        private static SlashCommandOptionBuilder AliasSubcommandGroup =>
            new SlashCommandOptionBuilder()
            .WithName("alias")
            .WithDescription("Alternative nicknames for a project")
            .WithNameLocalizations(GetCommandNames("project.alias"))
            .WithDescriptionLocalizations(GetCommandDescriptions("project.alias"))
            .WithType(ApplicationCommandOptionType.SubCommandGroup);

        private static SlashCommandOptionBuilder AddAliasSubcommand =>
            new SlashCommandOptionBuilder()
            .WithName("add")
            .WithDescription("Add a new alias")
            .WithNameLocalizations(GetCommandNames("project.alias.add"))
            .WithDescriptionLocalizations(GetCommandDescriptions("project.alias.add"))
            .WithType(ApplicationCommandOptionType.SubCommand)
            .AddOption(CommonOptions.Project())
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("alias")
                .WithDescription("Alias")
                .WithNameLocalizations(GetOptionNames("alias"))
                .WithDescriptionLocalizations(GetOptionDescriptions("alias"))
                .WithRequired(true)
                .WithType(ApplicationCommandOptionType.String)
            );

        private static SlashCommandOptionBuilder RemoveAliasSubcommand =>
            new SlashCommandOptionBuilder()
            .WithName("remove")
            .WithDescription("Remove an alias")
            .WithNameLocalizations(GetCommandNames("project.alias.remove"))
            .WithDescriptionLocalizations(GetCommandDescriptions("project.alias.remove"))
            .WithType(ApplicationCommandOptionType.SubCommand)
            .AddOption(CommonOptions.Project())
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("alias")
                .WithDescription("Alias")
                .WithNameLocalizations(GetOptionNames("alias"))
                .WithDescriptionLocalizations(GetOptionDescriptions("alias"))
                .WithRequired(true)
                .WithType(ApplicationCommandOptionType.String)
            );

        private static SlashCommandOptionBuilder AdminSubcommandGroup =>
            new SlashCommandOptionBuilder()
            .WithName("admin")
            .WithDescription("Project-level administrators")
            .WithNameLocalizations(GetCommandNames("project.admin"))
            .WithDescriptionLocalizations(GetCommandDescriptions("project.admin"))
            .WithType(ApplicationCommandOptionType.SubCommandGroup);

        private static SlashCommandOptionBuilder AddAdminSubcommand =>
            new SlashCommandOptionBuilder()
            .WithName("add")
            .WithDescription("Add an administrator to this project")
            .WithNameLocalizations(GetCommandNames("project.admin.add"))
            .WithDescriptionLocalizations(GetCommandDescriptions("project.admin.add"))
            .WithType(ApplicationCommandOptionType.SubCommand)
            .AddOption(CommonOptions.Project())
            .AddOption(CommonOptions.Member());

        private static SlashCommandOptionBuilder RemoveAdminSubcommand =>
            new SlashCommandOptionBuilder()
            .WithName("remove")
            .WithDescription("Remove an administrator from this project")
            .WithNameLocalizations(GetCommandNames("project.admin.remove"))
            .WithDescriptionLocalizations(GetCommandDescriptions("project.admin.remove"))
            .WithType(ApplicationCommandOptionType.SubCommand)
            .AddOption(CommonOptions.Project())
            .AddOption(CommonOptions.Member());

        private static SlashCommandOptionBuilder CongaSubcommandGroup =>
            new SlashCommandOptionBuilder()
            .WithName("conga")
            .WithDescription("A Conga line of Key Staff")
            .WithNameLocalizations(GetCommandNames("project.conga"))
            .WithDescriptionLocalizations(GetCommandDescriptions("project.conga"))
            .WithType(ApplicationCommandOptionType.SubCommandGroup);

        private static SlashCommandOptionBuilder AddCongaSubcommand =>
            new SlashCommandOptionBuilder()
            .WithName("add")
            .WithDescription("Add a link to the Conga line")
            .WithNameLocalizations(GetCommandNames("project.conga.add"))
            .WithDescriptionLocalizations(GetCommandDescriptions("project.conga.add"))
            .WithType(ApplicationCommandOptionType.SubCommand)
            .AddOption(CommonOptions.Project())
            .AddOption(CommonOptions.Abbreviation())
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("next")
                .WithDescription("Position to ping")
                .WithNameLocalizations(GetOptionNames("conga.next"))
                .WithDescriptionLocalizations(GetOptionDescriptions("conga.next"))
                .WithRequired(true)
                .WithAutocomplete(true)
                .WithType(ApplicationCommandOptionType.String)
            );

        private static SlashCommandOptionBuilder RemoveCongaSubcommand =>
            new SlashCommandOptionBuilder()
            .WithName("remove")
            .WithDescription("Remove a link from the Conga line")
            .WithNameLocalizations(GetCommandNames("project.conga.remove"))
            .WithDescriptionLocalizations(GetCommandDescriptions("project.conga.remove"))
            .WithType(ApplicationCommandOptionType.SubCommand)
            .AddOption(CommonOptions.Project())
            .AddOption(CommonOptions.Abbreviation());

        private static SlashCommandOptionBuilder ListCongaSubcommand =>
            new SlashCommandOptionBuilder()
            .WithName("list")
            .WithDescription("List all the Conga line participants")
            .WithNameLocalizations(GetCommandNames("project.conga.list"))
            .WithDescriptionLocalizations(GetCommandDescriptions("project.conga.list"))
            .WithType(ApplicationCommandOptionType.SubCommand)
            .AddOption(CommonOptions.Project());

        private static SlashCommandOptionBuilder AirReminderSubcommandGroup =>
            new SlashCommandOptionBuilder()
            .WithName("airreminder")
            .WithDescription("Enable or disable airing reminders")
            .WithNameLocalizations(GetCommandNames("project.airreminder"))
            .WithDescriptionLocalizations(GetCommandDescriptions("project.airreminder"))
            .WithType(ApplicationCommandOptionType.SubCommandGroup);

        private static SlashCommandOptionBuilder AirReminderEnableSubcommand =>
            new SlashCommandOptionBuilder()
            .WithName("enable")
            .WithDescription("Enable airing reminders")
            .WithNameLocalizations(GetCommandNames("project.airreminder.enable"))
            .WithDescriptionLocalizations(GetCommandDescriptions("project.airreminder.enable"))
            .WithType(ApplicationCommandOptionType.SubCommand)
            .AddOption(CommonOptions.Project())
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("channel")
                .WithDescription("Channel to post reminders in")
                .WithNameLocalizations(GetOptionNames("project.airreminder.channel"))
                .WithDescriptionLocalizations(GetOptionDescriptions("project.airreminder.channel"))
                .WithRequired(true)
                .WithType(ApplicationCommandOptionType.Channel)
            ).AddOption(new SlashCommandOptionBuilder()
                .WithName("role")
                .WithDescription("Role to ping for reminders")
                .WithNameLocalizations(GetOptionNames("project.airreminder.role"))
                .WithDescriptionLocalizations(GetOptionDescriptions("project.airreminder.role"))
                .WithRequired(false)
                .WithType(ApplicationCommandOptionType.Role)
            );

        private static SlashCommandOptionBuilder AirReminderDisableSubcommand =>
           new SlashCommandOptionBuilder()
           .WithName("disable")
           .WithDescription("Disable airing reminders")
           .WithNameLocalizations(GetCommandNames("project.airreminder.disable"))
           .WithDescriptionLocalizations(GetCommandDescriptions("project.airreminder.disable"))
           .WithType(ApplicationCommandOptionType.SubCommand)
           .AddOption(CommonOptions.Project());
    }
}
