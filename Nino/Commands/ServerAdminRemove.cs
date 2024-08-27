using Discord;
using Discord.WebSocket;
using Microsoft.Azure.Cosmos;
using Nino.Records;
using Nino.Utilities;

using static Localizer.Localizer;

namespace Nino.Commands
{
    internal static partial class ServerManagement
    {
        public static async Task<bool> HandleAdminRemove(SocketSlashCommand interaction, Configuration config)
        {
            var lng = interaction.UserLocale;
            var subcommand = interaction.Data.Options.First().Options.First();

            var memberId = ((SocketGuildUser)subcommand.Options.FirstOrDefault(o => o.Name == "member")!.Value).Id;
            var staffMention = $"<@{memberId}>";

            // Validate user is an admin
            if (!config.AdministratorIds.Any(a => a == memberId))
                return await Response.Fail(T("error.noSuchAdmin", lng, staffMention), interaction);

            var adminIndex = Array.IndexOf(config.AdministratorIds, config.AdministratorIds.Single(a => a == memberId));

            // Remove from database
            await AzureHelper.Configurations!.PatchItemAsync<Configuration>(id: config.Id, partitionKey: AzureHelper.ConfigurationPartitionKey(config),
                patchOperations: new[]
            {
                PatchOperation.Remove($"/administratorIds/{adminIndex}")
            });

            log.Info($"Updated configuration for guild {config.GuildId}, removed {memberId} as an administrator");

            // Send success embed
            var embed = new EmbedBuilder()
                .WithTitle(T("title.serverConfiguration", lng))
                .WithDescription(T("server.admin.removed", lng, staffMention))
                .Build();
            await interaction.FollowupAsync(embed: embed);

            return true;
        }
    }
}
