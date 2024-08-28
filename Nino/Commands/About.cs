using Discord;
using Discord.WebSocket;
using Nino.Utilities;

using static Localizer.Localizer;

namespace Nino.Commands
{
    internal static partial class About
    {
        public const string Name = "about";

        public static async Task<bool> Handle(SocketSlashCommand interaction)
        {
            var lng = interaction.UserLocale;

            var embed = new EmbedBuilder()
                .WithTitle(T("title.about", lng))
                .WithDescription(T("nino.about", lng, Utils.VERSION))
                .WithUrl("https://github.com/9vult/nino")
                .Build();
            await interaction.FollowupAsync(embed: embed);

            return true;
        }

        public static SlashCommandBuilder Builder =>
            new SlashCommandBuilder()
            .WithName(Name)
            .WithDescription("About Nino")
            .WithNameLocalizations(GetCommandNames(Name))
            .WithDescriptionLocalizations(GetCommandDescriptions(Name));
    }
}
