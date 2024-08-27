using Discord;
using Discord.WebSocket;
using Nino.Utilities;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static Localizer.Localizer;

namespace Nino.Commands
{
    internal static partial class Help
    {
        public const string Name = "help";

        public static async Task<bool> Handle(SocketSlashCommand interaction)
        {
            var lng = interaction.UserLocale;

            var embed = new EmbedBuilder()
                .WithTitle(T("title.help", lng))
                .WithDescription(T("nino.help", lng))
                .Build();
            await interaction.FollowupAsync(embed: embed);

            return true;
        }

        public static SlashCommandBuilder Builder =>
            new SlashCommandBuilder()
            .WithName(Name)
            .WithDescription("Nino Help")
            .WithNameLocalizations(GetCommandNames(Name))
            .WithDescriptionLocalizations(GetCommandDescriptions(Name));
    }
}
