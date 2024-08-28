using Discord;
using Discord.WebSocket;
using Nino.Records;

using static Localizer.Localizer;

namespace Nino.Commands
{
    internal static partial class ServerManagement
    {
        public static async Task<bool> HandleSetReleasePrefix(SocketSlashCommand interaction, Configuration config)
        {
            var lng = interaction.UserLocale;
            var subcommand = interaction.Data.Options.First();

            // Get inputs
            var newValue = (string)subcommand.Options.FirstOrDefault(o => o.Name == "newvalue")!.Value;
            var prefix = newValue == "-" ? null : newValue;

            // Apply change and upsert to database
            config.ReleasePrefix = prefix;


            await AzureHelper.Configurations!.UpsertItemAsync(config);
            log.Info($"Updated configuration for guild {config.GuildId}, set ReleasePrefix to {prefix ?? "(empty)"}");

            // Send success embed
            var embed = new EmbedBuilder()
                .WithTitle(T("title.serverConfiguration", lng))
                .WithDescription($"{T("server.configuration.saved", lng)}\n{T("info.resettable", lng)}")
                .Build();
            await interaction.FollowupAsync(embed: embed);

            return true;
        }

    }
}
