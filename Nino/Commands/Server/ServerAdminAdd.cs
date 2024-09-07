using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Azure.Cosmos;
using Nino.Records;
using Nino.Utilities;

using static Localizer.Localizer;

namespace Nino.Commands
{
    public partial class ServerManagement
    {
        public partial class Admin
        {
            [SlashCommand("add", "Add an administrator to this server")]
            public async Task<RuntimeResult> Add(
                [Summary("member", "Staff member")] SocketGuildUser member
            )
            {
                var interaction = Context.Interaction;
                var lng = interaction.UserLocale;
                var guildId = interaction.GuildId ?? 0;
                var guild = Nino.Client.GetGuild(guildId);

                // Validate inputs
                var memberId = member.Id;
                var staffMention = $"<@{memberId}>";

                // Server administrator permissions required
                var runner = guild.GetUser(interaction.User.Id);
                if (!runner.GuildPermissions.Administrator)
                    return await Response.Fail(T("error.notPrivileged", lng), interaction);

                var config = await Getters.GetConfiguration(guildId);
                if (config == null)
                    return await Response.Fail(T("error.noSuchConfig", lng), interaction);

                // Validate user isn't already an admin
                if (config.AdministratorIds.Any(a => a == memberId))
                    return await Response.Fail(T("error.admin.alreadyAdmin", lng, staffMention), interaction);

                // Add to database
                await AzureHelper.Configurations!.PatchItemAsync<Configuration>(id: config.Id, partitionKey: AzureHelper.ConfigurationPartitionKey(config),
                    patchOperations: [
                        PatchOperation.Add("/administratorIds/-", memberId.ToString())
                ]);

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
}
