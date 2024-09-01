using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Azure.Cosmos;
using Nino.Records;
using Nino.Utilities;

using static Localizer.Localizer;

namespace Nino.Commands
{
    internal static partial class ServerManagement
    {
        public static async Task<RuntimeResult> HandleAdminAdd(SocketSlashCommand interaction, Configuration config)
        {
            var lng = interaction.UserLocale;
            var subcommand = interaction.Data.Options.First().Options.First();

            var memberId = ((SocketGuildUser)subcommand.Options.FirstOrDefault(o => o.Name == "member")!.Value).Id;
            var staffMention = $"<@{memberId}>";

            // Validate user isn't already an admin
            if (config.AdministratorIds.Any(a => a == memberId))
                return await Response.Fail(T("error.admin.alreadyAdmin", lng, staffMention), interaction);

            // Add to database
            await AzureHelper.Configurations!.PatchItemAsync<Configuration>(id: config.Id, partitionKey: AzureHelper.ConfigurationPartitionKey(config),
                patchOperations: new[]
            {
                PatchOperation.Add("/administratorIds/-", memberId.ToString())
            });

            log.Info($"Updated configuration for guild {config.GuildId}, added {memberId} as an administrator");

            // Send success embed
            var embed = new EmbedBuilder()
                .WithTitle(T("title.serverConfiguration", lng))
                .WithDescription(T("server.admin.added", lng, staffMention))
                .Build();
            await interaction.FollowupAsync(embed: embed);

            await Cache.RebuildConfigCache();
            return ExecutionResult.Success;
        }
    }
}
