using Discord;
using Discord.WebSocket;
using Nino.Records;
using Nino.Records.Enums;

using static Localizer.Localizer;

namespace Nino.Commands
{
    internal static partial class ServerManagement
    {
        public static async Task<bool> HandleSetDisplayProgress(SocketSlashCommand interaction, Configuration config)
        {
            var lng = interaction.UserLocale;
            var subcommand = interaction.Data.Options.First().Options.First();

            // Get inputs
            var type = (DisplayType)Convert.ToInt32(subcommand.Options.FirstOrDefault(o => o.Name == "type")!.Value);

            // Apply change and upsert to database
            config.ProgressDisplay = type;

            await AzureHelper.Configurations!.UpsertItemAsync(config);
            log.Info($"Updated configuration for guild {config.GuildId}, set Progress Display to {type.ToFriendlyString()}");

            // Send success embed
            var embed = new EmbedBuilder()
                .WithTitle(T("title.serverConfiguration", lng))
                .WithDescription(T("server.configuration.saved", lng))
                .Build();
            await interaction.FollowupAsync(embed: embed);

            return true;
        }

    }
}
