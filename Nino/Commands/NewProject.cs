using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static Localizer.Localizer;

namespace Nino.Commands
{
    internal static class NewProject
    {
        public const string Name = "newproject";

        public static async Task Handle(SocketSlashCommand interaction)
        {
            await interaction.FollowupAsync("Test command response");
        }

        public static SlashCommandBuilder Builder =>
            new SlashCommandBuilder()
            .WithName(Name)
            .WithNameLocalizations(GetCommandNames(Name))
            .WithDescription(Name)
            .WithDescriptionLocalizations(GetCommandDescriptions(Name))
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
                .AddChoice("TV", "TV", GetChoiceNames("projecttype-TV"))
                .AddChoice("Movie", "Movie", GetChoiceNames("projecttype-Movie"))
                .AddChoice("BD", "BD", GetChoiceNames("projecttype-BD"))
                .WithType(ApplicationCommandOptionType.String)
            ).AddOption(new SlashCommandOptionBuilder()
                .WithName("length")
                .WithDescription("Number of episodes")
                .WithNameLocalizations(GetOptionNames("length"))
                .WithDescriptionLocalizations(GetOptionDescriptions("length"))
                .WithRequired(true)
                .WithMinValue(0)
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
            );
    }
}
