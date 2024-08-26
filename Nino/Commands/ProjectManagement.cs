using Discord;
using Discord.WebSocket;
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
            // Create
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("create")
                .WithDescription("Create a new project")
                .WithNameLocalizations(GetCommandNames("project.create"))
                .WithDescriptionLocalizations(GetCommandDescriptions("project.create"))
                .WithType(ApplicationCommandOptionType.SubCommand)
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("nickname")
                    .WithDescription("Project nickname")
                    .WithNameLocalizations(GetOptionNames("nickname"))
                    .WithDescriptionLocalizations(GetOptionDescriptions("nickname"))
                    .WithRequired(true)
                    .WithType(ApplicationCommandOptionType.String)
                ).AddOption(new SlashCommandOptionBuilder()
                    .WithName("title")
                    .WithDescription("Full series title")
                    .WithNameLocalizations(GetOptionNames("title"))
                    .WithDescriptionLocalizations(GetOptionDescriptions("title"))
                    .WithRequired(true)
                    .WithType(ApplicationCommandOptionType.String)
                ).AddOption(new SlashCommandOptionBuilder()
                    .WithName("projecttype")
                    .WithDescription("Project type")
                    .WithNameLocalizations(GetOptionNames("projecttype"))
                    .WithDescriptionLocalizations(GetOptionDescriptions("projecttype"))
                    .WithRequired(true)
                    .AddChoice("TV", 0, GetChoiceNames("projecttype-TV"))
                    .AddChoice("Movie", 1, GetChoiceNames("projecttype-Movie"))
                    .AddChoice("BD", 2, GetChoiceNames("projecttype-BD"))
                    .WithType(ApplicationCommandOptionType.Number)
                ).AddOption(new SlashCommandOptionBuilder()
                    .WithName("length")
                    .WithDescription("Number of episodes")
                    .WithNameLocalizations(GetOptionNames("length"))
                    .WithDescriptionLocalizations(GetOptionDescriptions("length"))
                    .WithRequired(true)
                    .WithMinValue(1)
                    .WithType(ApplicationCommandOptionType.Number)
                ).AddOption(new SlashCommandOptionBuilder()
                    .WithName("poster")
                    .WithDescription("Poster image URL")
                    .WithNameLocalizations(GetOptionNames("poster"))
                    .WithDescriptionLocalizations(GetOptionDescriptions("poster"))
                    .WithRequired(true)
                    .WithType(ApplicationCommandOptionType.String)
                ).AddOption(new SlashCommandOptionBuilder()
                    .WithName("private")
                    .WithDescription("Is this project private?")
                    .WithNameLocalizations(GetOptionNames("private"))
                    .WithDescriptionLocalizations(GetOptionDescriptions("private"))
                    .WithRequired(true)
                    .WithType(ApplicationCommandOptionType.Boolean)
                ).AddOption(new SlashCommandOptionBuilder()
                    .WithName("updatechannel")
                    .WithDescription("Channel to post updates to")
                    .WithNameLocalizations(GetOptionNames("updatechannel"))
                    .WithDescriptionLocalizations(GetOptionDescriptions("updatechannel"))
                    .WithRequired(true)
                    .WithType(ApplicationCommandOptionType.Channel)
                ).AddOption(new SlashCommandOptionBuilder()
                    .WithName("releasechannel")
                    .WithDescription("Channel to post releases to")
                    .WithNameLocalizations(GetOptionNames("releasechannel"))
                    .WithDescriptionLocalizations(GetOptionDescriptions("releasechannel"))
                    .WithRequired(true)
                    .WithType(ApplicationCommandOptionType.Channel)
                )
            )
            // Edit
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("edit")
                .WithDescription("Edit a project")
                .WithNameLocalizations(GetCommandNames("project.edit"))
                .WithDescriptionLocalizations(GetCommandDescriptions("project.edit"))
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
                    .WithName("option")
                    .WithDescription("Option to change")
                    .WithNameLocalizations(GetOptionNames("option"))
                    .WithDescriptionLocalizations(GetOptionDescriptions("option"))
                    .WithRequired(true)
                    .AddChoice("Title", 0, GetChoiceNames("editoption-title"))
                    .AddChoice("Poster", 1, GetChoiceNames("editoption-poster"))
                    .AddChoice("MOTD", 2, GetChoiceNames("editoption-motd"))
                    .AddChoice("AniDBId", 3, GetChoiceNames("editoption-anidb"))
                    .AddChoice("AirTime24h", 4, GetChoiceNames("editoption-airtime"))
                    .AddChoice("IsPrivate", 5, GetChoiceNames("editoption-private"))
                    .AddChoice("UpdateChannelID", 6, GetChoiceNames("editoption-updatechannel"))
                    .AddChoice("ReleaseChannelID", 7, GetChoiceNames("editoption-releasechannel"))
                    .WithType(ApplicationCommandOptionType.Number)
                ).AddOption(new SlashCommandOptionBuilder()
                    .WithName("newvalue")
                    .WithDescription("New Value")
                    .WithNameLocalizations(GetOptionNames("newvalue"))
                    .WithDescriptionLocalizations(GetOptionDescriptions("newvalue"))
                    .WithRequired(true)
                    .WithType(ApplicationCommandOptionType.String)
                )
            )
            // Delete
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("delete")
                .WithDescription("Delete a project")
                .WithNameLocalizations(GetCommandNames("project.delete"))
                .WithDescriptionLocalizations(GetCommandDescriptions("project.delete"))
                .WithType(ApplicationCommandOptionType.SubCommand)
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("project")
                    .WithDescription("Project nickname")
                    .WithNameLocalizations(GetOptionNames("project"))
                    .WithDescriptionLocalizations(GetOptionDescriptions("project"))
                    .WithRequired(true)
                    .WithAutocomplete(true)
                    .WithType(ApplicationCommandOptionType.String)
                )
            )
            // Transfer Ownership
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("transferownership")
                .WithDescription("Transfer project ownership to someone else")
                .WithNameLocalizations(GetCommandNames("project.transferownership"))
                .WithDescriptionLocalizations(GetCommandDescriptions("project.transferownership"))
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
                )
            )
            // Alias Management
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("alias")
                .WithDescription("Alternative nicknames for a project")
                .WithNameLocalizations(GetCommandNames("project.alias"))
                .WithDescriptionLocalizations(GetCommandDescriptions("project.alias"))
                .WithType(ApplicationCommandOptionType.SubCommandGroup)
                // Add Alias
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("add")
                    .WithDescription("Add a new alias")
                    .WithNameLocalizations(GetCommandNames("project.alias.add"))
                    .WithDescriptionLocalizations(GetCommandDescriptions("project.alias.add"))
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
                        .WithName("alias")
                        .WithDescription("Alias")
                        .WithNameLocalizations(GetOptionNames("alias"))
                        .WithDescriptionLocalizations(GetOptionDescriptions("alias"))
                        .WithRequired(true)
                        .WithType(ApplicationCommandOptionType.String)
                    )
                )
                // Remove Alias
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("remove")
                    .WithDescription("remove an alias")
                    .WithNameLocalizations(GetCommandNames("project.alias.remove"))
                    .WithDescriptionLocalizations(GetCommandDescriptions("project.alias.remove"))
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
                        .WithName("alias")
                        .WithDescription("Alias")
                        .WithNameLocalizations(GetOptionNames("alias"))
                        .WithDescriptionLocalizations(GetOptionDescriptions("alias"))
                        .WithRequired(true)
                        .WithType(ApplicationCommandOptionType.String)
                    )
                )
            );
    }
}
